using System;
using System.Collections.Generic;

namespace Geta.Commerce.ContentModelGenerator.Structure
{
    [Serializable]
    public class PropertyDefinition
    {
        public IList<AttributeDefinition> Attributes { get; set; }

        public string Name { get; set; } 
        public string Type { get; set; }
        
        public ProtectionLevel Protection { get; set; } = ProtectionLevel.Public;

        public bool Get { get; set; } = true;
        public bool Set { get; set; } = true;

        private bool _virtual = true;
        public bool Virtual
        {
            get { return _virtual; }
            set
            {
                if (value)
                {
                    _override = false;
                }

                _virtual = value;
            }
        }

        private bool _override;
        public bool Override
        {
            get { return _override; }
            set
            {
                if (value)
                {
                    _virtual = false;
                }

                _override = value;
            }
        }
    }
}