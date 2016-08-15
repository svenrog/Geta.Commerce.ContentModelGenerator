using System.Collections.Generic;

namespace Geta.Commerce.ContentModelGenerator.Structure
{
    public class PropertyDefinition
    {
        public IList<AttributeDefinition> Attributes { get; set; }

        public string Name { get; set; } 
        public string Type { get; set; }

        private ProtectionLevel _protection = ProtectionLevel.Public;
        public ProtectionLevel Protection { get { return _protection; } set { _protection = value; } }

        private bool _get = true;
        public bool Get { get { return _get; } set { _get = value; } }

        private bool _set = true;
        public bool Set { get { return _set; } set { _set = value; } }

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