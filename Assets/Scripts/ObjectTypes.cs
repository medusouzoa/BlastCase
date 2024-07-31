using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "NewObjectType", menuName = "ScriptableObjects/ObjectType", order = 1)]
    public class ObjectTypes : ScriptableObject
    {
        //0 -> Blue
        //1 -> Green
        //2 -> Pink
        //3 -> Purple
        //4 -> Red
        //5 -> Yellow

        public ColoredObject[] coloredObjects;

        [Serializable]
        public class ColoredObject
        {
            public Sprite pieceSprite;
            public int color;
            public int type;
        }

        public Sprite GetSpriteForColor(int colorValue, int typeValue)
        {
            return (from pair in coloredObjects
                where
                    pair.color == colorValue && pair.type == typeValue
                select
                    pair.pieceSprite).FirstOrDefault();
        }
    }
}