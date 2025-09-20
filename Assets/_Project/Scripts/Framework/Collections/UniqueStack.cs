using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace VTBeat.Collection {
    public partial class UniqueStack<T> {
        private readonly LinkedList<T> m_LinkedList;
        private readonly Dictionary<T, LinkedListNode<T>> m_Item2Node;
        
        public UniqueStack() : this(new List<T>()) { }
        public UniqueStack(IEnumerable<T> collection) {
            m_LinkedList = new LinkedList<T>();
            m_Item2Node = new Dictionary<T, LinkedListNode<T>>();
            
            foreach (T item in collection) {
                Add(item);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T item) {
            Remove(item);
            Add(item);
        }
        public bool TryPeek(out T view) {
            view = default;
            if (m_LinkedList.First == null) return false;
            
            view = m_LinkedList.First.Value;
            return true;
        }
        public T Pop() {
            if (!TryPeek(out T view)) return default;
            
            Remove(view);
            return view;
        }
        
        public int IndexOf(T item) {
            if (item == null || !m_Item2Node.ContainsKey(item)) {
                return -1;
            }
            
            int index = 0;
            foreach (T nodeValue in m_LinkedList) {
                if (EqualityComparer<T>.Default.Equals(nodeValue, item)) {
                    return index;
                }
                index++;
            }
            
            return -1;
        }
        public void Insert(int index, T item) {
            if (index < 0 || index > Count) throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative and less than or equal to the size of the collection.");
            
            int oldCount = -1;
            if (Contains(item)) {
                oldCount = IndexOf(item);
                Remove(item);
            }
            
            if (oldCount != -1 && oldCount < index) {
                index--;
            }
            
            if (index == Count) {
                Add(item);
                return;
            }
            
            LinkedListNode<T> current = m_LinkedList.First;
            for (int i = 0; i < index; i++) {
                current = current.Next;
            }
            
            LinkedListNode<T> newNode = m_LinkedList.AddBefore(current, item);
            m_Item2Node.Add(item, newNode);
        }
    }
    
    public partial class UniqueStack<T> : ICollection<T> {
        public int Count => m_LinkedList.Count;
        public bool IsReadOnly => false;
        
        public void Add(T item) {
            if (item == null) return;
            if (Contains(item)) return;
            
            LinkedListNode<T> newNode = m_LinkedList.AddLast(item);
            m_Item2Node.Add(item, newNode);
        }
        public void Clear() {
            m_LinkedList.Clear();
            m_Item2Node.Clear();
        }
        public bool Contains(T item) {
            return item != null && m_Item2Node.ContainsKey(item);
        }
        public void CopyTo(T[] array, int arrayIndex) {
            m_LinkedList.CopyTo(array, arrayIndex);
        }
        public bool Remove(T item) {
            if (!m_Item2Node.Remove(item, out LinkedListNode<T> node)) return false;
            
            m_LinkedList.Remove(node);
            return true;
        }
        
        public IEnumerator<T> GetEnumerator() => m_LinkedList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    public partial class UniqueStack<T> : IReadOnlyCollection<T> { } // Shared with ICollection<T>
}