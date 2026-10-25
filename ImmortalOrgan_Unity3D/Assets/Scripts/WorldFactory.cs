using System;
using UnityEngine;

namespace ImmortalOrgan
{
    public static class WorldFactory
    {
        private static Shader mobileShader;

        public static Material MakeMaterial(Color color, bool emissive = false)
        {
            // Runtime-only Shader.Find references can be stripped from Android
            // players. A Shader asset in Resources is explicitly included.
            if (mobileShader == null)
                mobileShader = Resources.Load<Shader>("ImmortalOrganMobile");

            Shader shader = mobileShader;
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null)
                throw new InvalidOperationException("ImmortalOrganMobile shader is missing from the Android player.");

            var mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (!emissive && mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", .13f);
            if (!emissive && mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", .13f);
            if (emissive && mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 1.35f);
            }
            return mat;
        }

        public static Transform Part(string name, PrimitiveType type, Transform parent,
            Vector3 position, Vector3 scale, Material material, bool collider = false)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider && go.TryGetComponent<Collider>(out var shape))
                UnityEngine.Object.Destroy(shape);
            return go.transform;
        }

        public static void ConfigureAtmosphere()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.70f, .80f, .81f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = .012f;
            RenderSettings.fogColor = new Color(.62f, .79f, .78f);
            RenderSettings.reflectionIntensity = .20f;
        }

        public static void BuildLandscape(Transform root)
        {
            Transform environment = new GameObject("1 · 침식의 숲 / Environment").transform;
            environment.SetParent(root);
            var grass = MakeMaterial(new Color(.37f, .62f, .46f));
            var field = MakeMaterial(new Color(.54f, .68f, .46f));
            var trail = MakeMaterial(new Color(.69f, .65f, .48f));
            var bark = MakeMaterial(new Color(.32f, .22f, .19f));
            var foliageA = MakeMaterial(new Color(.20f, .50f, .39f));
            var foliageB = MakeMaterial(new Color(.39f, .65f, .45f));
            var foliageC = MakeMaterial(new Color(.33f, .57f, .52f));
            var rock = MakeMaterial(new Color(.49f, .57f, .57f));
            var ruin = MakeMaterial(new Color(.43f, .54f, .56f));
            var paleRuin = MakeMaterial(new Color(.68f, .73f, .68f));
            var blood = MakeMaterial(new Color(.82f, .18f, .33f), true);

            // A 200 x 200 metre environment, entirely generated from primitives.
            Part("Collidable ground", PrimitiveType.Plane, environment,
                Vector3.zero, new Vector3(20f, 1f, 20f), grass, true);

            Path(environment, trail, new Vector3(0f, .02f, -30f), new Vector3(-8f, .02f, 4f), 6f);
            Path(environment, trail, new Vector3(-8f, .02f, 4f), new Vector3(8f, .02f, 22f), 5.6f);
            Path(environment, trail, new Vector3(8f, .02f, 22f), new Vector3(0f, .02f, 64f), 6f);

            Part("Sunlit clearing", PrimitiveType.Cylinder, environment,
                new Vector3(-16f, -.05f, 15f), new Vector3(18f, .06f, 18f), field);
            Part("Small clearing", PrimitiveType.Cylinder, environment,
                new Vector3(22f, -.05f, 35f), new Vector3(14f, .06f, 14f), field);

            var random = new System.Random(1776);
            for (int i = 0; i < 115; i++)
            {
                float x = (float)(random.NextDouble() * 160 - 80);
                float z = (float)(random.NextDouble() * 170 - 75);
                Vector3 at = new Vector3(x, 0f, z);
                if (Vector3.Distance(at, new Vector3(0f, 0f, -25f)) < 9f) continue;
                if (Mathf.Abs(x) < 8f && z > 42f && z < 75f) continue;
                if (NearQuestMarker(at)) continue;

                float size = 0.75f + (float)random.NextDouble() * .65f;
                Material leaves = i % 3 == 0 ? foliageA : (i % 3 == 1 ? foliageB : foliageC);
                Tree(environment, at, size, bark, leaves);
            }

            for (int i = 0; i < 55; i++)
            {
                float x = (float)(random.NextDouble() * 150 - 75);
                float z = (float)(random.NextDouble() * 160 - 70);
                float size = .35f + (float)random.NextDouble() * 1.1f;
                Part("Weathered boulder", PrimitiveType.Sphere, environment,
                    new Vector3(x, size * .23f, z), new Vector3(size, size * .48f, size * .8f), rock);
            }

            BuildLaboratory(environment, ruin, paleRuin, blood);

            var sun = new GameObject("Warm morning light");
            sun.transform.SetParent(environment);
            sun.transform.rotation = Quaternion.Euler(49f, -37f, 0f);
            Light directional = sun.AddComponent<Light>();
            directional.type = LightType.Directional;
            directional.color = new Color(1f, .91f, .77f);
            directional.intensity = 1.15f;
            directional.shadows = LightShadows.Soft;

            // Distant silhouettes give the test zone a sense of scale.
            var hill = MakeMaterial(new Color(.34f, .57f, .52f));
            for (int i = 0; i < 12; i++)
            {
                float theta = i * Mathf.PI * 2f / 12f;
                Part("Distant forest ridge", PrimitiveType.Sphere, environment,
                    new Vector3(Mathf.Cos(theta) * 96f, -7f, Mathf.Sin(theta) * 96f),
                    new Vector3(28f, 18f, 24f), hill);
            }
        }

        private static bool NearQuestMarker(Vector3 position)
        {
            return Vector3.Distance(position, new Vector3(-14f, 0f, 6f)) < 6f ||
                   Vector3.Distance(position, new Vector3(19f, 0f, 21f)) < 6f ||
                   Vector3.Distance(position, new Vector3(-9f, 0f, 43f)) < 6f;
        }

        private static void Path(Transform parent, Material surface, Vector3 a, Vector3 b, float width)
        {
            Vector3 delta = b - a;
            Transform slab = Part("Walkable trail", PrimitiveType.Cube, parent,
                (a + b) * .5f, new Vector3(width, .08f, delta.magnitude), surface);
            slab.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
        }

        private static void Tree(Transform parent, Vector3 position, float size, Material bark, Material leaf)
        {
            Transform root = new GameObject("Korean forest tree").transform;
            root.SetParent(parent);
            root.position = position;
            Part("Trunk", PrimitiveType.Cylinder, root,
                new Vector3(0f, size * 1.25f, 0f), new Vector3(.30f * size, 1.25f * size, .30f * size), bark, true);
            Part("Tree canopy", PrimitiveType.Sphere, root,
                new Vector3(0f, size * 3.10f, 0f), new Vector3(2.0f, 2.2f, 2.0f) * size, leaf);
            Part("Secondary canopy", PrimitiveType.Sphere, root,
                new Vector3(-.75f * size, size * 2.65f, .26f * size),
                new Vector3(1.35f, 1.25f, 1.25f) * size, leaf);
        }

        private static void BuildLaboratory(Transform parent, Material stone, Material pale, Material blood)
        {
            Transform lab = new GameObject("Abandoned Lazarus Laboratory / Restricted").transform;
            lab.SetParent(parent);
            lab.position = new Vector3(0f, 0f, 68f);
            Part("Main block", PrimitiveType.Cube, lab,
                new Vector3(0, 3, 2), new Vector3(17f, 6f, 13f), stone, true);
            Part("Upper wing", PrimitiveType.Cube, lab,
                new Vector3(-4, 7, 2), new Vector3(9f, 2.2f, 11f), pale, true);
            Part("Entrance opening", PrimitiveType.Cube, lab,
                new Vector3(0, 2.3f, -4.61f), new Vector3(4.5f, 4.3f, .35f),
                MakeMaterial(new Color(.09f, .20f, .23f)));

            for (int i = 0; i < 4; i++)
            {
                float x = -6f + i * 4f;
                Part("Glazed windows", PrimitiveType.Cube, lab,
                    new Vector3(x, 4.2f, -4.7f), new Vector3(1.7f, 1.7f, .2f),
                    MakeMaterial(new Color(.21f, .55f, .60f), true));
            }

            Part("L-Cell containment beam", PrimitiveType.Cylinder, lab,
                new Vector3(0, 2.4f, -7f), new Vector3(.20f, 2.1f, .20f), blood);
            Part("Warning marker", PrimitiveType.Sphere, lab,
                new Vector3(0, 4.8f, -7f), new Vector3(.55f, .55f, .55f), blood);
            for (int i = -1; i <= 1; i++)
            {
                Part("Obsolete pipes", PrimitiveType.Cylinder, lab,
                    new Vector3(i * 4f, 1.8f, 6.5f), new Vector3(.18f, 2f, .18f), pale);
            }
        }

        public static PlayerMotor CreatePlayer(Transform parent)
        {
            GameObject player = new GameObject("CHOI MIN-HO | Player");
            player.transform.SetParent(parent);
            player.transform.position = new Vector3(0f, 1.1f, -28f);
            player.layer = 8;
            player.AddComponent<CharacterController>();
            PlayerMotor controller = player.AddComponent<PlayerMotor>();
            player.AddComponent<BloodCombat>();
            var visual = player.AddComponent<AnimeCharacter>();
            visual.Build();
            return controller;
        }

        public static OrbitCamera CreateCamera(Transform parent, Transform target)
        {
            Camera camera = Camera.main;
            GameObject go;
            if (camera == null)
            {
                go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                camera = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            else go = camera.gameObject;

            go.transform.SetParent(parent);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.62f, .79f, .80f);
            camera.fieldOfView = 60f;
            camera.nearClipPlane = .08f;
            camera.farClipPlane = 230f;
            OrbitCamera orbit = go.GetComponent<OrbitCamera>();
            if (orbit == null) orbit = go.AddComponent<OrbitCamera>();
            orbit.Target = target;
            return orbit;
        }

        public static void PlaceChallenges(Transform parent, QuestDirector quest)
        {
            Transform gameplay = new GameObject("2 · L-Cell Quest / Creatures").transform;
            gameplay.SetParent(parent);
            CreateShard(gameplay, quest, new Vector3(-14f, 1.35f, 6f));
            CreateShard(gameplay, quest, new Vector3(19f, 1.35f, 21f));
            CreateShard(gameplay, quest, new Vector3(-9f, 1.35f, 43f));

            CreateEnemy(gameplay, new Vector3(-9f, 0f, -1f), false);
            CreateEnemy(gameplay, new Vector3(5f, 0f, 12f), false);
            CreateEnemy(gameplay, new Vector3(13f, 0f, 24f), false);
            CreateEnemy(gameplay, new Vector3(-16f, 0f, 38f), false);
            CreateEnemy(gameplay, new Vector3(4f, 0f, 45f), false);
            EnemyAgent boss = CreateEnemy(gameplay, new Vector3(0f, 0f, 53f), true);
            quest.RegisterGuardian(boss);
        }

        private static void CreateShard(Transform parent, QuestDirector quest, Vector3 position)
        {
            Transform root = new GameObject("L-Cell research trace").transform;
            root.SetParent(parent);
            root.position = position;
            Material cyan = MakeMaterial(new Color(.42f, 1f, .85f), true);
            Material white = MakeMaterial(new Color(.89f, 1f, .92f), true);
            Part("Shimmering cellular core", PrimitiveType.Sphere, root,
                Vector3.zero, new Vector3(.58f, .82f, .58f), cyan);
            Part("Inner nucleus", PrimitiveType.Sphere, root,
                Vector3.zero, Vector3.one * .23f, white);
            ShardPickup pickup = root.gameObject.AddComponent<ShardPickup>();
            quest.RegisterPickup(pickup);
            var light = root.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(.39f, 1f, .83f);
            light.intensity = 1.2f;
            light.range = 4f;
        }

        private static EnemyAgent CreateEnemy(Transform parent, Vector3 position, bool boss)
        {
            GameObject root = new GameObject(boss ? "CORE ECHO | Guardian" : "Corrupted watcher");
            root.transform.SetParent(parent);
            root.transform.position = position;
            CapsuleCollider collider = root.AddComponent<CapsuleCollider>();
            var enemy = root.AddComponent<EnemyAgent>();
            enemy.IsGuardian = boss;
            enemy.MaxHealth = boss ? 290f : 65f;
            enemy.AttackDamage = boss ? 17f : 9f;
            enemy.MovementSpeed = boss ? 2.9f : 2.3f;
            enemy.DetectionRadius = boss ? 18f : 12f;

            Transform figure = new GameObject("Chitin silhouette").transform;
            figure.SetParent(root.transform, false);
            Material dark = MakeMaterial(boss ? new Color(.24f, .13f, .29f) : new Color(.27f, .20f, .29f));
            Material red = MakeMaterial(new Color(.95f, .13f, .36f), true);
            Material bone = MakeMaterial(new Color(.55f, .52f, .65f));
            float size = boss ? 1.55f : 1f;
            Part("Infected body", PrimitiveType.Capsule, figure,
                new Vector3(0f, size * .97f, 0f), new Vector3(.95f, 1.0f, .73f) * size, dark);
            Part("Chitin helmet", PrimitiveType.Sphere, figure,
                new Vector3(0f, size * 1.58f, .07f), new Vector3(.94f, .70f, .80f) * size, dark);
            for (int s = -1; s <= 1; s += 2)
            {
                Part("Red sensory eye", PrimitiveType.Sphere, figure,
                    new Vector3(s * .22f * size, 1.56f * size, .46f * size),
                    new Vector3(.21f, .12f, .11f) * size, red);
                Part("Corrupted rib", PrimitiveType.Cube, figure,
                    new Vector3(s * .32f * size, .98f * size, .34f * size),
                    new Vector3(.11f, .65f, .14f) * size, bone);
            }

            Transform barRoot = new GameObject("Health meter (world space)").transform;
            barRoot.SetParent(root.transform, false);
            barRoot.localPosition = new Vector3(0f, boss ? 3.9f : 2.35f, 0f);
            Part("Bar backing", PrimitiveType.Cube, barRoot,
                Vector3.zero, new Vector3(1.2f, .08f, .10f), MakeMaterial(new Color(.13f, .16f, .20f)));
            Transform fill = Part("Health fill", PrimitiveType.Cube, barRoot,
                new Vector3(0, 0, -.07f), new Vector3(1.1f, .07f, .08f), red);
            enemy.Initialize(figure, fill);
            return enemy;
        }
    }
}
