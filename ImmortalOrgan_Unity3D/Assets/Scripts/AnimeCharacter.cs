using UnityEngine;

namespace ImmortalOrgan
{
    /// <summary>
    /// A placeholder 3D humanoid styled after the original Choi Min-ho concept art.
    /// Replace this procedural mesh with a production-ready rigged model later.
    /// </summary>
    public sealed class AnimeCharacter : MonoBehaviour
    {
        private PlayerMotor motor;
        private Transform visualRoot;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;
        private Transform coatLeft;
        private Transform coatRight;
        private Transform bloodOrb;
        private float slashTime;

        public void Build()
        {
            motor = GetComponent<PlayerMotor>();
            var body = new GameObject("Minho | Stylized body");
            visualRoot = body.transform;
            visualRoot.SetParent(transform, false);

            var coat = WorldFactory.MakeMaterial(new Color(.09f, .12f, .20f));
            var coatShine = WorldFactory.MakeMaterial(new Color(.21f, .25f, .33f));
            var black = WorldFactory.MakeMaterial(new Color(.06f, .07f, .11f));
            var skin = WorldFactory.MakeMaterial(new Color(.89f, .73f, .68f));
            var crimson = WorldFactory.MakeMaterial(new Color(.84f, .17f, .31f), true);
            var steel = WorldFactory.MakeMaterial(new Color(.59f, .65f, .70f));

            WorldFactory.Part("Torso", PrimitiveType.Capsule, visualRoot,
                new Vector3(0f, 1.28f, 0f), new Vector3(.63f, .84f, .40f), coat);
            WorldFactory.Part("Center vest", PrimitiveType.Cube, visualRoot,
                new Vector3(0f, 1.22f, .20f), new Vector3(.30f, .48f, .08f), coatShine);
            WorldFactory.Part("Blood line", PrimitiveType.Cube, visualRoot,
                new Vector3(-.07f, 1.28f, .25f), new Vector3(.035f, .44f, .04f), crimson);

            WorldFactory.Part("Neck", PrimitiveType.Cylinder, visualRoot,
                new Vector3(0f, 1.77f, 0f), new Vector3(.14f, .11f, .14f), skin);
            WorldFactory.Part("Face", PrimitiveType.Sphere, visualRoot,
                new Vector3(0f, 1.96f, .02f), new Vector3(.43f, .55f, .39f), skin);
            WorldFactory.Part("Hair crown", PrimitiveType.Sphere, visualRoot,
                new Vector3(0f, 2.18f, -.05f), new Vector3(.49f, .31f, .45f), black);
            WorldFactory.Part("Asymmetric fringe", PrimitiveType.Capsule, visualRoot,
                new Vector3(-.16f, 2.11f, .24f), new Vector3(.16f, .24f, .13f), black).localRotation = Quaternion.Euler(0f, 0f, 26f);
            WorldFactory.Part("Ash fringe", PrimitiveType.Capsule, visualRoot,
                new Vector3(.15f, 2.14f, .23f), new Vector3(.11f, .22f, .12f), steel).localRotation = Quaternion.Euler(0f, 0f, -34f);

            var eye = WorldFactory.MakeMaterial(new Color(.44f, .08f, .13f), true);
            WorldFactory.Part("Eye L", PrimitiveType.Sphere, visualRoot,
                new Vector3(-.11f, 1.99f, .21f), new Vector3(.065f, .048f, .03f), eye);
            WorldFactory.Part("Eye R", PrimitiveType.Sphere, visualRoot,
                new Vector3(.11f, 1.99f, .21f), new Vector3(.065f, .048f, .03f), eye);

            leftArm = Limb("Left arm", new Vector3(-.39f, 1.56f, 0f), coat,
                new Vector3(.21f, .60f, .21f));
            rightArm = Limb("Right arm", new Vector3(.39f, 1.56f, 0f), coat,
                new Vector3(.21f, .60f, .21f));
            WorldFactory.Part("Left glove", PrimitiveType.Sphere, leftArm,
                new Vector3(0f, -.39f, 0f), new Vector3(.22f, .23f, .22f), black);
            WorldFactory.Part("Right glove", PrimitiveType.Sphere, rightArm,
                new Vector3(0f, -.39f, 0f), new Vector3(.22f, .23f, .22f), black);
            WorldFactory.Part("Erosion mark", PrimitiveType.Cube, leftArm,
                new Vector3(0f, -.18f, .12f), new Vector3(.07f, .35f, .04f), crimson);

            leftLeg = Limb("Left leg", new Vector3(-.17f, .83f, 0f), black,
                new Vector3(.24f, .70f, .27f));
            rightLeg = Limb("Right leg", new Vector3(.17f, .83f, 0f), black,
                new Vector3(.24f, .70f, .27f));
            WorldFactory.Part("Boot L", PrimitiveType.Cube, leftLeg,
                new Vector3(0f, -.42f, .08f), new Vector3(.28f, .21f, .43f), black);
            WorldFactory.Part("Boot R", PrimitiveType.Cube, rightLeg,
                new Vector3(0f, -.42f, .08f), new Vector3(.28f, .21f, .43f), black);

            coatLeft = WorldFactory.Part("Coat tail L", PrimitiveType.Cube, visualRoot,
                new Vector3(-.25f, .69f, -.17f), new Vector3(.36f, .95f, .11f), coat);
            coatRight = WorldFactory.Part("Coat tail R", PrimitiveType.Cube, visualRoot,
                new Vector3(.25f, .67f, -.17f), new Vector3(.36f, .99f, .11f), coatShine);
            bloodOrb = WorldFactory.Part("Unstable L-Cell", PrimitiveType.Sphere, leftArm,
                new Vector3(0f, -.46f, .12f), new Vector3(.16f, .16f, .16f), crimson);
        }

        private Transform Limb(string name, Vector3 origin, Material mat, Vector3 scale)
        {
            Transform pivot = new GameObject(name + " pivot").transform;
            pivot.SetParent(visualRoot, false);
            pivot.localPosition = origin;
            WorldFactory.Part(name, PrimitiveType.Capsule, pivot,
                new Vector3(0f, -.26f, 0f), scale, mat);
            return pivot;
        }

        public void PlaySlash() => slashTime = .25f;

        private void Update()
        {
            if (visualRoot == null || motor == null) return;
            slashTime = Mathf.Max(0f, slashTime - Time.deltaTime);
            float gait = motor.IsMoving ? Mathf.Sin(motor.MoveCycle) * 25f : 0f;
            float idle = Mathf.Sin(Time.time * 2.2f) * .025f;
            visualRoot.localPosition = Vector3.up * idle;
            leftLeg.localRotation = Quaternion.Euler(gait, 0, 0);
            rightLeg.localRotation = Quaternion.Euler(-gait, 0, 0);
            leftArm.localRotation = Quaternion.Euler(-gait * .45f - slashTime * 460f, 0f, -8f);
            rightArm.localRotation = Quaternion.Euler(gait * .45f, 0f, 8f);
            coatLeft.localRotation = Quaternion.Euler(motor.IsMoving ? -12f : -5f, 0f, 12f);
            coatRight.localRotation = Quaternion.Euler(motor.IsMoving ? -16f : -7f, 0f, -10f);
            float s = .7f + Mathf.Sin(Time.time * 5f) * .16f;
            bloodOrb.localScale = Vector3.one * s;
        }
    }
}
