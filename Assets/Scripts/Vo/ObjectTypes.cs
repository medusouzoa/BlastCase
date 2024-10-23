﻿using Enum;
using System;
using System.Linq;
using UnityEngine;

namespace Vo
{
    [CreateAssetMenu(fileName = "NewObjectType", menuName = "ScriptableObjects/ObjectType", order = 1)]
    public class ObjectTypes : ScriptableObject
    {
        //1 -> Blue
        //2 -> Green
        //3 -> Pink
        //4 -> Purple
        //5 -> Red
        //6 -> Yellow

        public ColoredObject[] coloredObjects;
        public BoxObject[] boxObjects;
        public BlastableObject[] blastableObjects;

        [Serializable]
        public class ColoredObject
        {
            public Sprite pieceSprite;
            public int color;
            public int type;
        }

        [Serializable]
        public class BoxObject
        {
            public Sprite pieceSprite;
            public int health;
        }
        [Serializable]
        public class BlastableObject
        {
            public Sprite pieceSprite;
            public BlastableType type;
        }
        public Sprite GetSpriteForColor(int colorValue, int typeValue)
        {
            return (from pair in coloredObjects
                    where
                        pair.color == colorValue && pair.type == typeValue
                    select
                        pair.pieceSprite).FirstOrDefault();
        }

        public Sprite GetSpriteForBox(int health)
        {
            return (from pair in boxObjects
                    where
                        pair.health == health
                    select
                        pair.pieceSprite).FirstOrDefault();
        }
        public Sprite GetSpriteForType(BlastableType type)
        {
            return (from pair in blastableObjects
                    where
                        pair.type == type
                    select
                        pair.pieceSprite).FirstOrDefault();
        }
    }
}