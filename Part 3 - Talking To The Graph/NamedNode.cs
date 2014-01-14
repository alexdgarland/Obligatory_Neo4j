using System.Collections;
using Neo4jClient;          // Add by using Nuget Package Manager ("Install-Package Neo4jClient")

namespace Neo4jWidgetManager
{


    abstract class NamedNode
    {
        public string Name { get; private set; }
        protected GraphClient client;

        public NamedNode(string Name, GraphClient client)
        {
            this.Name = Name;
            this.client = client;
            createGraphNode();
        }

        private void createGraphNode()
        {
            client.Cypher
                .Create("(n: " + NodeType + " {Name: { NodeName }})")
                .WithParam("NodeName", this.Name)
                .ExecuteWithoutResults();
        }

        public abstract string NodeType { get; }
    }

	
    class WidgetNode : NamedNode
    {
        public WidgetNode(string Name, GraphClient client) : base(Name, client) { }
        public override string NodeType { get { return "Widget"; } }

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

        private class ComponentResult
        {
            public string Name;
        }
    }



    class ComponentNode : NamedNode
    {
        public ComponentNode(string Name, GraphClient client) : base(Name, client) { }
        public override string NodeType { get { return "Component"; } }

    }
	
	
}
