using System.Collections.Generic;
using System.IO;
using System.Text;
using Geta.Commerce.ContentModelGenerator.Builders;

namespace Geta.Commerce.ContentModelGenerator.Parsers
{
    /// <summary>
    /// A simple class parser that handles a single namespace and class inside a file
    /// </summary>
    public class ClassParser
    {
        protected ParserState CurrentState;

        protected string ClassName;
        protected string ClassNameSpace;
        protected string ClassInherits;

        public ClassBuilder ParseFile(string fileName, Stream stream)
        {
            CurrentState = ParserState.Namespace;

            using (var reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                while (!reader.EndOfStream)
                {
                    switch (CurrentState)
                    {
                        case ParserState.Includes : ReadIncludes(reader); break;
                        case ParserState.Namespace : ReadNamespace(reader); break;
                        case ParserState.Class : ReadClass(reader); break;
                        case ParserState.Members : ReadMembers(reader); break;
                    }
                }
            }

            return new ClassBuilder(string.Empty, string.Empty);
        }

        protected virtual void ReadIncludes(StreamReader reader)
        {
            
        }

        protected virtual void ReadNamespace(StreamReader reader)
        {
            
        }

        protected virtual void ReadClass(StreamReader reader)
        {
            
        }

        protected virtual void ReadMembers(StreamReader reader)
        {
            
        }

        protected virtual string ReadUntil(StreamReader reader, char character)
        {
            return ReadUntil(reader, new HashSet<char>{ character });
        }

        protected virtual string ReadUntil(StreamReader reader, ISet<char> characters)
        {
            var builder = new StringBuilder();
            var buffer = new char[1];

            while (!reader.EndOfStream && !characters.Contains(buffer[0]))
            {
                reader.Read(buffer, 0, 1);
                builder.Append(buffer);
            }
            
            return builder.ToString();
        }
    }
}