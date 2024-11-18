using System.Collections.Generic;

namespace BehaviourTree
{
    public interface IStrategy
    {
        Node.Status Precess();
        void Reset();
    }


    // you can creaet more children classes from Node class
    // the basic Node calsss in brhaviour tree are
    // Leaf, Selector and Sequence
    public class Inverter : Node
    {
        public Inverter(string name) : base(name) { }

        public override Status Process()
        {
            switch (children[0].Process())
            {
                case Status.Success:
                    return Status.Failure;
                case Status.Failure: 
                    return Status.Success;
                default:
                    return Status.Failure;
            }
        }

    }

    public class Selector : Node
    {
        public Selector(string name) : base(name) { }

        public override Status Process()
        {
            if (currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Success:
                        Reset();
                        return Status.Success;
                    default:
                        currentChild++;
                        return Status.Running;
                }
            }

            Reset();
            return Status.Failure;
        }
    }
    public class Sequence : Node
    {
        public Sequence(string name) : base(name) { }

        public override Status Process()
        {
            if (currentChild < children.Count)
            {
                switch (children[currentChild].Process())
                {
                    case Status.Running:
                        return Status.Running;
                    case Status.Failure:
                        Reset();
                        return Status.Failure;
                    default:
                        currentChild++;
                        return currentChild == children.Count ? Status.Success : Status.Running;
                }
            }

            Reset();
            return Status.Failure;
        }
    }

    public class BehaviourTree : Node
    {
        public BehaviourTree(string name) : base(name) { }

        public override Status Process()
        {
            while (currentChild < children.Count)
            {
                var status = children[currentChild].Process();
                if(status != Status.Success)
                {
                    return status;
                }
                currentChild++;
            }
            return Status.Success;
        }
    }

    public class Leaf : Node
    {
        readonly IStrategy strategy;

        public Leaf(string name, IStrategy strategy) : base(name)
        {
            // Preconditions.CheckNotNull(strategy);
            this.strategy = strategy; 
        }

        public override Status Process() => strategy.Precess();

        public override void Reset() => strategy.Reset();
    }
    public class Node
    {
        public enum Status { Success, Failure, Running };

        public readonly string name;

        public readonly List<Node> children = null;
        protected int currentChild;

        public Node(string name = "Node")
        {
            this.name = name;
        }

        public void AddChild(Node child) => children.Add(child);

        public virtual Status Process() => children[currentChild].Process();

        public virtual void Reset()
        {
            currentChild = 0;
            foreach(var child in children)
            {
                child.Reset();
            }
        }

    }
}
