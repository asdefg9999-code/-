using System.Collections.Generic;
using UnityEngine;

namespace ImmortalOrgan
{
    /// <summary>
    /// Low-poly, original Min-ho prototype with a real skinned mesh, bind poses,
    /// and independently animated bones. No editor-generated asset is needed for
    /// the Android build. An authored MinhoRigged prefab always takes priority.
    /// This is a rig prototype, not the final modeled/UV-painted FBX character.
    /// </summary>
    public sealed class MinhoRuntimeRig : MonoBehaviour
    {
        private const int Pelvis = 0;
        private const int Spine = 1;
        private const int Chest = 2;
        private const int Neck = 3;
        private const int Head = 4;
        private const int ArmL = 5;
        private const int ForearmL = 6;
        private const int HandL = 7;
        private const int ArmR = 8;
        private const int ForearmR = 9;
        private const int HandR = 10;
        private const int ThighL = 11;
        private const int ShinL = 12;
        private const int FootL = 13;
        private const int ThighR = 14;
        private const int ShinR = 15;
        private const int FootR = 16;
        private const int TailL = 17;
        private const int TailR = 18;

        private const int Coat = 0;
        private const int Trim = 1;
        private const int Dark = 2;
        private const int Skin = 3;
        private const int Blood = 4;
        private const int Metal = 5;
        private const int Lining = 6;
        private const int Hair = 7;

        private readonly Transform[] bones = new Transform[19];
        private PlayerMotor motor;
        private CharacterController controller;
        private Mesh generatedMesh;
        private Material[] generatedMaterials;
        private float slashTime;

        public void Build(PlayerMotor playerMotor, CharacterController playerController)
        {
            motor = playerMotor;
            controller = playerController;
            CreateSkeleton();

            var geometry = new RigGeometry(8);
            CreateBody(geometry);
            CreateFaceAndHair(geometry);
            CreateCoat(geometry);
            generatedMesh = geometry.ToMesh(bones, transform);
            generatedMesh.name = "Minho Runtime Rig | Skinned Mesh";

            generatedMaterials = new[]
            {
                WorldFactory.MakeMaterial(new Color(.07f, .105f, .17f)),
                WorldFactory.MakeMaterial(new Color(.18f, .22f, .32f)),
                WorldFactory.MakeMaterial(new Color(.035f, .041f, .065f)),
                WorldFactory.MakeMaterial(new Color(.92f, .75f, .70f)),
                WorldFactory.MakeMaterial(new Color(.76f, .085f, .17f), true),
                WorldFactory.MakeMaterial(new Color(.67f, .73f, .79f)),
                WorldFactory.MakeMaterial(new Color(.38f, .07f, .14f)),
                WorldFactory.MakeMaterial(new Color(.085f, .082f, .11f))
            };

            var skin = gameObject.AddComponent<SkinnedMeshRenderer>();
            skin.sharedMesh = generatedMesh;
            skin.sharedMaterials = generatedMaterials;
            skin.bones = bones;
            skin.rootBone = bones[Pelvis];
            skin.updateWhenOffscreen = false;
            skin.localBounds = new Bounds(new Vector3(0f, .95f, 0f), new Vector3(2.2f, 2.6f, 1.5f));
            skin.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            skin.receiveShadows = true;
        }

        public void PlaySlash()
        {
            slashTime = .32f;
        }

        private void CreateSkeleton()
        {
            bones[Pelvis] = Bone("Hips", transform, new Vector3(0f, .90f, 0f));
            bones[Spine] = Bone("Spine", bones[Pelvis], new Vector3(0f, .22f, 0f));
            bones[Chest] = Bone("Chest", bones[Spine], new Vector3(0f, .28f, 0f));
            bones[Neck] = Bone("Neck", bones[Chest], new Vector3(0f, .18f, 0f));
            bones[Head] = Bone("Head", bones[Neck], new Vector3(0f, .13f, 0f));

            bones[ArmL] = Bone("LeftUpperArm", bones[Chest], new Vector3(-.32f, .01f, 0f));
            bones[ForearmL] = Bone("LeftLowerArm", bones[ArmL], new Vector3(0f, -.27f, 0f));
            bones[HandL] = Bone("LeftHand", bones[ForearmL], new Vector3(0f, -.25f, 0f));
            bones[ArmR] = Bone("RightUpperArm", bones[Chest], new Vector3(.32f, .01f, 0f));
            bones[ForearmR] = Bone("RightLowerArm", bones[ArmR], new Vector3(0f, -.27f, 0f));
            bones[HandR] = Bone("RightHand", bones[ForearmR], new Vector3(0f, -.25f, 0f));

            bones[ThighL] = Bone("LeftUpperLeg", bones[Pelvis], new Vector3(-.15f, -.04f, 0f));
            bones[ShinL] = Bone("LeftLowerLeg", bones[ThighL], new Vector3(0f, -.39f, 0f));
            bones[FootL] = Bone("LeftFoot", bones[ShinL], new Vector3(0f, -.38f, .04f));
            bones[ThighR] = Bone("RightUpperLeg", bones[Pelvis], new Vector3(.15f, -.04f, 0f));
            bones[ShinR] = Bone("RightLowerLeg", bones[ThighR], new Vector3(0f, -.39f, 0f));
            bones[FootR] = Bone("RightFoot", bones[ShinR], new Vector3(0f, -.38f, .04f));
            bones[TailL] = Bone("LeftCoatTail", bones[Pelvis], new Vector3(-.2f, -.02f, -.16f));
            bones[TailR] = Bone("RightCoatTail", bones[Pelvis], new Vector3(.2f, -.02f, -.16f));
        }

        private static Transform Bone(string name, Transform parent, Vector3 position)
        {
            var bone = new GameObject(name).transform;
            bone.SetParent(parent, false);
            bone.localPosition = position;
            bone.localRotation = Quaternion.identity;
            return bone;
        }

        private static void CreateBody(RigGeometry g)
        {
            g.Tube(new Vector3(0f, .85f, 0f), new Vector3(0f, 1.43f, 0f),
                .22f, .285f, .15f, .175f, Pelvis, Chest, Coat);
            g.Tube(new Vector3(0f, 1.42f, 0f), new Vector3(0f, 1.61f, 0f),
                .075f, .065f, .075f, .067f, Chest, Neck, Skin);

            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * .32f;
                int upper = side < 0 ? ArmL : ArmR;
                int lower = side < 0 ? ForearmL : ForearmR;
                int hand = side < 0 ? HandL : HandR;
                int thigh = side < 0 ? ThighL : ThighR;
                int shin = side < 0 ? ShinL : ShinR;
                int foot = side < 0 ? FootL : FootR;

                g.Tube(new Vector3(x, .90f, 0f), new Vector3(x, 1.42f, 0f),
                    .072f, .105f, .075f, .105f, lower, upper, Coat);
                g.Tube(new Vector3(x, .87f, 0f), new Vector3(x, .97f, 0f),
                    .082f, .078f, .083f, .083f, hand, lower, side < 0 ? Blood : Dark);
                g.Tube(new Vector3(x, .09f, 0f), new Vector3(x, .87f, 0f),
                    .11f, .125f, .11f, .13f, shin, thigh, Dark);
                g.Tube(new Vector3(x, .08f, .035f), new Vector3(x, .31f, .035f),
                    .12f, .118f, .14f, .125f, foot, shin, Coat);
                g.Ellipsoid(new Vector3(x, .89f, 0f), new Vector3(.093f, .13f, .09f), Skin, hand);
            }

            // Coat lapels, blood filaments, central chest sigil and belt hardware.
            g.Tube(new Vector3(-.10f, 1.19f, .18f), new Vector3(-.17f, 1.44f, .15f),
                .017f, .024f, .012f, .016f, Spine, Chest, Trim);
            g.Tube(new Vector3(.10f, 1.19f, .18f), new Vector3(.17f, 1.44f, .15f),
                .017f, .024f, .012f, .016f, Spine, Chest, Trim);
            g.Tube(new Vector3(-.08f, 1.22f, .17f), new Vector3(.055f, 1.43f, .17f),
                .012f, .014f, .012f, .014f, Spine, Chest, Blood);
            g.Tube(new Vector3(.075f, 1.23f, .17f), new Vector3(-.055f, 1.40f, .18f),
                .010f, .012f, .011f, .013f, Spine, Chest, Blood);
            g.Ellipsoid(new Vector3(0f, 1.29f, .177f), new Vector3(.038f, .047f, .014f), Metal, Spine);
            g.Ellipsoid(new Vector3(0f, .90f, .155f), new Vector3(.065f, .035f, .022f), Metal, Pelvis);
            g.Tube(new Vector3(-.37f, .98f, .045f), new Vector3(-.29f, 1.23f, .085f),
                .012f, .012f, .009f, .009f, ForearmL, ArmL, Blood);
        }

        private static void CreateFaceAndHair(RigGeometry g)
        {
            g.Ellipsoid(new Vector3(0f, 1.72f, .016f), new Vector3(.168f, .205f, .134f), Skin, Head);
            g.Ellipsoid(new Vector3(0f, 1.838f, -.026f), new Vector3(.186f, .13f, .157f), Hair, Head);
            g.Ellipsoid(new Vector3(-.074f, 1.729f, .146f), new Vector3(.034f, .020f, .012f), Blood, Head);
            g.Ellipsoid(new Vector3(.074f, 1.729f, .146f), new Vector3(.034f, .020f, .012f), Blood, Head);
            g.Tube(new Vector3(-.11f, 1.78f, .141f), new Vector3(-.037f, 1.79f, .146f),
                .006f, .006f, .006f, .006f, Head, Head, Hair);
            g.Tube(new Vector3(.037f, 1.79f, .146f), new Vector3(.11f, 1.78f, .141f),
                .006f, .006f, .006f, .006f, Head, Head, Hair);
            g.Ellipsoid(new Vector3(0f, 1.668f, .145f), new Vector3(.029f, .008f, .006f), Trim, Head);

            // Distinctive swept raven hair and silver side fringe from the 3-view sheet.
            g.Cone(new Vector3(-.13f, 1.88f, .11f), new Vector3(-.11f, 1.65f, .146f), .095f, Head, Hair);
            g.Cone(new Vector3(-.015f, 1.91f, .09f), new Vector3(-.065f, 1.73f, .161f), .086f, Head, Hair);
            g.Cone(new Vector3(.12f, 1.89f, .112f), new Vector3(.092f, 1.675f, .155f), .087f, Head, Metal);
            g.Cone(new Vector3(.17f, 1.84f, .03f), new Vector3(.205f, 1.72f, .09f), .067f, Head, Hair);
            g.Cone(new Vector3(-.18f, 1.84f, .02f), new Vector3(-.215f, 1.74f, .08f), .067f, Head, Hair);
        }

        private static void CreateCoat(RigGeometry g)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                int tail = side < 0 ? TailL : TailR;
                float s = side;
                g.Panel(new Vector3(s * .05f, .91f, -.17f), new Vector3(s * .35f, .89f, -.15f),
                    new Vector3(s * .47f, .19f, -.26f), new Vector3(s * .11f, .30f, -.23f),
                    Pelvis, tail, Coat, Lining);
                g.Panel(new Vector3(s * .18f, .90f, .14f), new Vector3(s * .31f, .90f, .055f),
                    new Vector3(s * .36f, .34f, .06f), new Vector3(s * .16f, .38f, .13f),
                    Pelvis, tail, Trim, Lining);
            }
        }

        private void Update()
        {
            if (motor == null || bones[Pelvis] == null) return;
            float dt = Time.deltaTime;
            slashTime = Mathf.Max(0f, slashTime - dt);
            float strength = motor.IsMoving ? 1f : 0f;
            float gait = Mathf.Sin(motor.MoveCycle) * 30f * strength;
            float breath = Mathf.Sin(Time.time * 2.25f);
            bool airborne = controller != null && !controller.isGrounded;
            float dash = motor.IsDashing ? 1f : 0f;
            float slash = slashTime > 0f ? Mathf.Sin((.32f - slashTime) / .32f * Mathf.PI) : 0f;

            bones[Pelvis].localPosition = new Vector3(0f, .90f + breath * .006f, 0f);
            bones[Pelvis].localRotation = Quaternion.Euler(0f, 0f, breath * 1.5f);
            bones[Spine].localRotation = Quaternion.Euler(dash * 13f + (airborne ? -5f : 0f), 0f, breath * 1.7f);
            bones[Chest].localRotation = Quaternion.Euler(dash * 9f, slash * -24f, breath * -.8f);
            bones[Head].localRotation = Quaternion.Euler(-dash * 7f, breath * 2f, breath * -.8f);

            bones[ArmL].localRotation = Quaternion.Euler(-gait * .60f - slash * 103f,
                slash * -22f, -7f - dash * 20f);
            bones[ForearmL].localRotation = Quaternion.Euler(-10f - slash * 70f, 0f, 0f);
            bones[ArmR].localRotation = Quaternion.Euler(gait * .60f + slash * 18f,
                0f, 7f + dash * 20f);
            bones[ForearmR].localRotation = Quaternion.Euler(-6f - dash * 20f, 0f, 0f);

            bones[ThighL].localRotation = Quaternion.Euler(airborne ? -17f : gait, 0f, 0f);
            bones[ThighR].localRotation = Quaternion.Euler(airborne ? 13f : -gait, 0f, 0f);
            bones[ShinL].localRotation = Quaternion.Euler(airborne ? 27f : Mathf.Max(0f, -gait) * 0.5f, 0f, 0f);
            bones[ShinR].localRotation = Quaternion.Euler(airborne ? 13f : Mathf.Max(0f, gait) * 0.5f, 0f, 0f);
            bones[TailL].localRotation = Quaternion.Euler(-6f - strength * 12f - dash * 20f, 0f, 8f + gait * .2f);
            bones[TailR].localRotation = Quaternion.Euler(-6f - strength * 12f - dash * 20f, 0f, -8f - gait * .2f);
        }

        private void OnDestroy()
        {
            if (generatedMesh != null) Destroy(generatedMesh);
            if (generatedMaterials == null) return;
            foreach (Material material in generatedMaterials)
                if (material != null) Destroy(material);
        }

        private sealed class RigGeometry
        {
            private readonly List<Vector3> vertices = new List<Vector3>(1500);
            private readonly List<Vector2> uvs = new List<Vector2>(1500);
            private readonly List<BoneWeight> weights = new List<BoneWeight>(1500);
            private readonly List<int>[] triangles;

            public RigGeometry(int materials)
            {
                triangles = new List<int>[materials];
                for (int i = 0; i < materials; i++) triangles[i] = new List<int>();
            }

            private static BoneWeight Weight(int first, int second, float secondBlend)
            {
                float t = Mathf.Clamp01(secondBlend);
                return new BoneWeight
                {
                    boneIndex0 = first,
                    weight0 = 1f - t,
                    boneIndex1 = second,
                    weight1 = t
                };
            }

            private void Vertex(Vector3 p, Vector2 uv, BoneWeight weight)
            {
                vertices.Add(p);
                uvs.Add(uv);
                weights.Add(weight);
            }

            public void Tube(Vector3 bottom, Vector3 top, float radiusX0, float radiusX1,
                float radiusZ0, float radiusZ1, int boneBottom, int boneTop, int material)
            {
                const int sides = 10;
                const int rings = 4;
                int start = vertices.Count;
                for (int ring = 0; ring < rings; ring++)
                {
                    float t = ring / (float)(rings - 1);
                    Vector3 center = Vector3.Lerp(bottom, top, t);
                    float rx = Mathf.Lerp(radiusX0, radiusX1, t);
                    float rz = Mathf.Lerp(radiusZ0, radiusZ1, t);
                    BoneWeight weight = Weight(boneBottom, boneTop, Mathf.SmoothStep(0f, 1f, t));
                    for (int side = 0; side < sides; side++)
                    {
                        float angle = side * Mathf.PI * 2f / sides;
                        Vertex(center + new Vector3(Mathf.Cos(angle) * rx, 0f, Mathf.Sin(angle) * rz),
                            new Vector2(side / (float)sides, t), weight);
                    }
                }

                for (int ring = 0; ring < rings - 1; ring++)
                    for (int side = 0; side < sides; side++)
                    {
                        int next = (side + 1) % sides;
                        int a = start + ring * sides + side;
                        int b = start + (ring + 1) * sides + side;
                        int c = start + ring * sides + next;
                        int d = start + (ring + 1) * sides + next;
                        Face(material, a, b, c);
                        Face(material, c, b, d);
                    }
            }

            public void Ellipsoid(Vector3 center, Vector3 radii, int material, int bone)
            {
                const int sides = 12;
                const int levels = 7;
                int start = vertices.Count;
                for (int level = 0; level <= levels; level++)
                {
                    float lat = -Mathf.PI * .5f + level * Mathf.PI / levels;
                    float ringR = Mathf.Cos(lat);
                    for (int side = 0; side < sides; side++)
                    {
                        float angle = side * Mathf.PI * 2f / sides;
                        Vertex(center + new Vector3(Mathf.Cos(angle) * ringR * radii.x,
                                Mathf.Sin(lat) * radii.y, Mathf.Sin(angle) * ringR * radii.z),
                            new Vector2(side / (float)sides, level / (float)levels), Weight(bone, bone, 0f));
                    }
                }
                for (int level = 0; level < levels; level++)
                    for (int side = 0; side < sides; side++)
                    {
                        int next = (side + 1) % sides;
                        int a = start + level * sides + side;
                        int b = start + (level + 1) * sides + side;
                        int c = start + level * sides + next;
                        int d = start + (level + 1) * sides + next;
                        Face(material, a, b, c);
                        Face(material, c, b, d);
                    }
            }

            public void Cone(Vector3 top, Vector3 tip, float radius, int bone, int material)
            {
                const int sides = 9;
                int start = vertices.Count;
                for (int i = 0; i < sides; i++)
                {
                    float angle = i * Mathf.PI * 2f / sides;
                    Vertex(top + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius),
                        new Vector2(i / (float)sides, 0f), Weight(bone, bone, 0f));
                }
                Vertex(tip, new Vector2(.5f, 1f), Weight(bone, bone, 0f));
                for (int i = 0; i < sides; i++) Face(material, start + i, start + sides, start + (i + 1) % sides);
            }

            public void Panel(Vector3 upperLeft, Vector3 upperRight, Vector3 lowerRight, Vector3 lowerLeft,
                int upperBone, int lowerBone, int outerMaterial, int innerMaterial)
            {
                int start = vertices.Count;
                Vertex(upperLeft, new Vector2(0f, 1f), Weight(upperBone, lowerBone, .03f));
                Vertex(upperRight, new Vector2(1f, 1f), Weight(upperBone, lowerBone, .03f));
                Vertex(lowerRight, new Vector2(1f, 0f), Weight(upperBone, lowerBone, .95f));
                Vertex(lowerLeft, new Vector2(0f, 0f), Weight(upperBone, lowerBone, .95f));
                Face(outerMaterial, start, start + 1, start + 2);
                Face(outerMaterial, start, start + 2, start + 3);
                Face(innerMaterial, start + 2, start + 1, start);
                Face(innerMaterial, start + 3, start + 2, start);
            }

            private void Face(int material, int a, int b, int c)
            {
                triangles[material].Add(a);
                triangles[material].Add(b);
                triangles[material].Add(c);
            }

            public Mesh ToMesh(Transform[] bones, Transform root)
            {
                var mesh = new Mesh();
                mesh.SetVertices(vertices);
                mesh.SetUVs(0, uvs);
                mesh.boneWeights = weights.ToArray();
                var bindposes = new Matrix4x4[bones.Length];
                for (int i = 0; i < bones.Length; i++)
                    bindposes[i] = bones[i].worldToLocalMatrix * root.localToWorldMatrix;
                mesh.bindposes = bindposes;
                mesh.subMeshCount = triangles.Length;
                for (int i = 0; i < triangles.Length; i++) mesh.SetTriangles(triangles[i], i);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
