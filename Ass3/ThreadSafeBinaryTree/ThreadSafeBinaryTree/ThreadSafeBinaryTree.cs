using System;
using System.Collections.Generic;
using System.Threading;

namespace ThreadedBinarySearchTree
{
    public interface IBinaryTree
    {
        void Add(string item);
        void Delete(string item);
        int Search(string item);
        void PrintSorted();
    }

    public class ThreadSafeBinaryTree : IBinaryTree
    {
        private Node root;
        private ReaderWriterLockSlim treeLock;

        public ThreadSafeBinaryTree()
        {
            root = null;
            treeLock = new ReaderWriterLockSlim();
        }

        public void Add(string item)
        {
            treeLock.EnterWriteLock();
            try
            {
                root = AddRecursive(root, item);
            }
            finally
            {
                treeLock.ExitWriteLock();
            }
        }

        private Node AddRecursive(Node current, string item)
        {
            if (current == null)
            {
                return new Node(item);
            }

            int compareResult = string.Compare(item, current.Value, StringComparison.Ordinal);
            if (compareResult < 0)
            {
                current.Left = AddRecursive(current.Left, item);
            }
            else if (compareResult > 0)
            {
                current.Right = AddRecursive(current.Right, item);
            }
            else
            {
                current.ReferenceCount++;
            }

            return current;
        }

        public void Delete(string item)
        {
            treeLock.EnterWriteLock();
            try
            {
                root = DeleteRecursive(root, item);
            }
            finally
            {
                treeLock.ExitWriteLock();
            }
        }

        private Node DeleteRecursive(Node current, string item)
        {
            if (current == null)
            {
                return null;
            }

            int compareResult = string.Compare(item, current.Value, StringComparison.Ordinal);
            if (compareResult < 0)
            {
                current.Left = DeleteRecursive(current.Left, item);
            }
            else if (compareResult > 0)
            {
                current.Right = DeleteRecursive(current.Right, item);
            }
            else
            {
                current.ReferenceCount--;
                if (current.ReferenceCount == 0)
                {
                    if (current.Left == null)
                    {
                        return current.Right;
                    }
                    else if (current.Right == null)
                    {
                        return current.Left;
                    }

                    current.Value = MinValue(current.Right);
                    current.Right = DeleteRecursive(current.Right, current.Value);
                }
            }

            return current;
        }

        private string MinValue(Node node)
        {
            string minValue = node.Value;
            while (node.Left != null)
            {
                minValue = node.Left.Value;
                node = node.Left;
            }
            return minValue;
        }

        public int Search(string item)
        {
            treeLock.EnterReadLock();
            try
            {
                return SearchRecursive(root, item);
            }
            finally
            {
                treeLock.ExitReadLock();
            }
        }

        private int SearchRecursive(Node current, string item)
        {
            if (current == null)
            {
                return 0;
            }

            int compareResult = string.Compare(item, current.Value, StringComparison.Ordinal);
            if (compareResult == 0)
            {
                return current.ReferenceCount;
            }

            return compareResult < 0
                ? SearchRecursive(current.Left, item)
                : SearchRecursive(current.Right, item);
        }

        public void PrintSorted()
        {
            treeLock.EnterReadLock();
            try
            {
                PrintSortedRecursive(root);
            }
            finally
            {
                treeLock.ExitReadLock();
            }
        }

        private void PrintSortedRecursive(Node current)
        {
            if (current != null)
            {
                PrintSortedRecursive(current.Left);
                Console.WriteLine($"{current.Value} ({current.ReferenceCount})");
                PrintSortedRecursive(current.Right);
            }
        }

        private class Node
        {
            public string Value { get; set; }
            public int ReferenceCount { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }

            public Node(string value)
            {
                Value = value;
                ReferenceCount = 1;
                Left = null;
                Right = null;
            }
        }
    }
}