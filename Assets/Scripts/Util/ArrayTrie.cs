using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace StlVault.Util
{
    public class ArrayTrie
    {
        private readonly Node _head = new Node(ReadOnlySpan<char>.Empty);
        private int _maxLength;
        private bool _anyInserted;

        internal class Node
        {
            public int Occurrences;
            public bool IsTerminal => Occurrences > 0;
            public (char, Node)[] Children;

            public Node(ReadOnlySpan<char> word)
            {
                if (word.IsEmpty) Occurrences = 1;
                else Children = new[] {(word[0], new Node(word.Slice(1)))};
            }
        }

        [PublicAPI]
        public bool Insert(string word) => Insert(word.AsSpan());
        
        [PublicAPI]
        public void Insert(IReadOnlyCollection<string> tags)
        {
            foreach (var tag in tags) Insert(tag);
        }
        
        [PublicAPI]
        public bool Insert(ReadOnlySpan<char> word)
        {
            if (word.IsEmpty) ThrowHelper.CantStoreEmptyWord(nameof(word));
            _anyInserted = true;
            
            _maxLength = Math.Max(_maxLength, word.Length);

            var current = _head;
            while (!word.IsEmpty)
            {
                var startChar = word[0];
                var restOfWord = word.Slice(1);

                // No children? Create array and store chars
                if (current.Children == null)
                {
                    current.Children = new[] {(startChar, new Node(restOfWord))};
                    return true;
                }

                // Can't find child starting with startChar? Append to array
                if (!FindChild(current.Children, startChar, out var childNode))
                {
                    AppendChild(ref current.Children, (startChar, new Node(restOfWord)));
                    return true;
                }

                // Rinse and repeat while start of word is known
                current = childNode;
                word = restOfWord;
            }

            // No early exit: either return false because word was known...
            if (current.IsTerminal)
            {
                current.Occurrences++;
                return false;
            }

            // ... or set terminal if we added a shorter, unknown word
            current.Occurrences = 1;
            return true;
        }

        [PublicAPI, SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
        public IEnumerable<(string word, int occurrences)> Find(string start)
        {
            if(!_anyInserted) yield break;
            
            // Walk down the trie to the subtree of words starting with "start"
            var current = _head;
            foreach (var key in start)
            {
                if (!FindChild(current.Children, key, out current)) yield break;
            }

            // Current might already be a stored word => return that
            if (current.IsTerminal) yield return (start, current.Occurrences);

            // From here on out, we need to build a word -> fill start of buffer
            var buffer = new char[_maxLength];
            start.AsSpan().CopyTo(buffer);

            foreach (var result in SearchTerminalNodes(current, buffer, start.Length))
            {
                yield return result;
            }
        }

        private static IEnumerable<(string word, int occurrences)> SearchTerminalNodes(Node node, char[] buffer,
            int position)
        {
            if (node.Children == null) yield break;

            foreach (var (key, childNode) in node.Children)
            {
                buffer[position] = key;

                if (childNode.IsTerminal)
                {
                    yield return (new string(buffer, 0, position + 1), childNode.Occurrences);
                }

                foreach (var word in SearchTerminalNodes(childNode, buffer, position + 1))
                {
                    yield return word;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool FindChild((char, Node)[] data, char search, out Node node)
        {
            foreach (var item in data)
            {
                char key;
                (key, node) = item;
                if (key == search) return true;
            }

            node = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendChild(ref (char, Node)[] data, (char, Node) child)
        {
            Array.Resize(ref data, data.Length + 1);
            data[data.Length - 1] = child;
        }

        private static class ThrowHelper
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void CantStoreEmptyWord(string paramName)
            {
                throw new ArgumentException("Can't store empty words!", paramName);
            }
        }
    }
}