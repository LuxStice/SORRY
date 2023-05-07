using UnityEngine;

namespace SORRY.Modules
{
    public class SizesUI
    {
        public static readonly float[] Sizes = { 0.325f, 0.625f, 0.9375f, 1.25f, 1.875f, 2.5f, 3.125f, 3.75f, 4.375f, 5 };
        public static (int size, float visualSize) GetSize(float toGet)
        {
            switch (toGet)
            {
                case 0.325f:
                    return new(0, 0f);
                case 0.625f:
                    return new(0, 0.25f);
                case 0.9235f:
                    return new(0, 0.5f);
                case 1.25f:
                    return new(1, 1);
                case 1.875f:
                    return new(1, 1.5f);
                case 2.5f:
                    return new(2, 2f);
                case 3.125f:
                    return new(2, 2.5f);
                case 3.75f:
                    return new(3, 3f);
                case 4.375f:
                    return new(3, 3.5f);
                case 5f:
                    return new(4, 4f);
                default:
                    return default;
            }
        }
        private int currentIndex = 4;
        private int maxIndex = Sizes.Length;
        public float Value => Sizes[currentIndex];

        public SizesUI()
        {

        }

        public SizesUI(float diameter)
        {
            int index = -1;
            float difference = float.MaxValue;

            for (int i = 0; i < Sizes.Length; i++)
            {
                if (Math.Abs(diameter - Sizes[i]) < difference)
                {
                    difference = Math.Abs(diameter - Sizes[i]);
                    index = i;
                }
            }
            currentIndex = index;
        }

        public bool DrawGUI()
        {
            bool changed = false;
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("<"))
            {
                currentIndex--;
                if (currentIndex < 0)
                    currentIndex = Sizes.Length - 1;
                changed = true;
            }

            GUILayout.Label(Sizes[currentIndex].ToString("0.0000m"), new GUIStyle(SpaceWarp.API.UI.Skins.ConsoleSkin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });

            if (GUILayout.Button(">"))
            {
                currentIndex++;
                if (currentIndex >= maxIndex)
                    currentIndex = 0;

                changed = true;
            }

            GUILayout.EndHorizontal();
            return changed;
        }

        internal void SetLimit(int size)
        {
            maxIndex = size;
            if (currentIndex > maxIndex)
                currentIndex = maxIndex - 1;
        }
    }
}