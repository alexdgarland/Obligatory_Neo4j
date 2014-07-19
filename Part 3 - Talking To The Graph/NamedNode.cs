/*
 * 
 * Example code for blog post alexdgarland.com/2014/01/17/obligatory-neo4j-3/
 * Trying out Neo4jClient  https://github.com/Readify/Neo4jClient/wiki
 * Please be aware that this is no way intended to demonstrate best practice
 * for working with this client or Neo4j in general!
 *
 * Alex Garland 16/01/2014
 * 
 */

/*
 * This code file provides classes for use in the program flow defined by EnterpriseWidgetManager.cs.
 */

using System.Collections;
using Neo4jClient;          //Add by using Nuget Package Manager ("Install-Package Neo4jClient")

namespace Neo4jWidgetManager
{
    abstract class NamedNode
    {
        // Base class for potentially ** anything ** that has a name and needs to be saved to the graph.

        public string Name { get; private set; }
        protected GraphClient client;
        public abstract string NodeType { get; }

        public NamedNode(string Name, GraphClient client)
        {
            this.Name = Name;
            this.client = client;
            createGraphNode();
            // Writing the data to the graph in the constructor - just for simplicity.
            // In a real app I guess this would be delayed,  working with data in memory
            // until the cost of network and IO use is justified by confirmed persistence requirements.
        }

        private void createGraphNode()
        {
            client.Cypher
                .Create("(n: " + NodeType + " {Name: { NodeName }})")
                .WithParam("NodeName", this.Name)
                .ExecuteWithoutResults();
            // The string concatenation here to add node type/ label is DEFINITELY not best practice,
            // but I didn't manage to use a parameter for this as I have for Node Name.
            // This is at least a case where the type might well not be freely user-supplied,
            // so minimising injection vulnerability.
        }

        
    }

    class WidgetNode : NamedNode
    {
        // Specific sub-class that represents widgets -
        // chose to place additional method "AddComponent" here for creating relationships.

        public WidgetNode(string Name, GraphClient client) : base(Name, client) { }

        public override string NodeType
        {
            get { return "Widget"; }
        }

        public void AddComponent(ComponentNode component)
        {
            client.Cypher
                .Match("(c:Component)", "(w:Widget)")
                .Where((ComponentNode c) => c.Name == component.Name)
                .AndWhere((WidgetNode w) => w.Name == this.Name)
                .Create("c-[:USED_IN]->w")
                .ExecuteWithoutResults();
        }

        public static IEnumerable GetSharedComponents(WidgetNode widget1, WidgetNode widget2, GraphClient client)
        {
            return client.Cypher
                    .Match("(w1{Name:{name1}})<-[:USED_IN]-(c:Component)-[:USED_IN]->(w2{Name:{name2}})")
                    .WithParam("name1", widget1.Name)
                    .WithParam("name2", widget2.Name)
                    .Return(c => c.As<ComponentResult>().Name)
                    .Results;
        }

        // Didn't fancy altering Component class to have a default constructor
        // (necessary for Neo4jClient node deserialisation),
        // so this nested class is a least-worst hack to save me abandoning the specific API I wanted.
        private class ComponentResult
        {
            public string Name;
        }
    }



    class ComponentNode : NamedNode
    {
        // Specific sub-class that represents components
        public ComponentNode(string Name, GraphClient client) : base(Name, client) { }

        public override string NodeType
        {
            get { return "Component"; }
        }
    }
}
