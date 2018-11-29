﻿using System;

namespace Geta.Commerce.ContentModelGenerator.Structure
{
    [Serializable]
    public enum CommerceContentType
    {
         Product = 0,
         Variation = 1,
         Category = 2,
         Bundle = 3,
         Package = 4,
         Entry = 5
    }
}