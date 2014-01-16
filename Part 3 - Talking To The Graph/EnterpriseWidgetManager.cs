/*
 * 
 * Example code for blog post http://alexdgarland.wordpress.com/2014/01/14/obligatory-neo4j-3
 * Trying out Neo4jClient  https://github.com/Readify/Neo4jClient/wiki
 * Please be aware that this is no way intended to demonstrate best practice for working with this client or Neo4j in general!
 *
 * Alex Garland 16/01/2014
 * 
 */

/*
 * This code file deals mainly with program flow; connections to the API/ database are mainly handled through the simple class hierarchy in NamedNode.cs.
 */

using System;
using System.Collections;
using Neo4jClient;          //Add by using Nuget Package Manager ("Install-Package Neo4jClient")

namespace Neo4jWidgetManager
{
    class EnterpriseWidgetManager
    {
        static void Main(string[] args)
        {
            // Set up connection
            var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
            client.Connect();

            // Clear down database of widgets, components and usage relationships
            ClearDown(client);

            // Create widgets
            var widgetA = new WidgetNode("WidgetA", client);
            var widgetB = new WidgetNode("WidgetB", client);
            Console.WriteLine("Widgets Created");

            // Create components
            var Component1 = new ComponentNode("Component1", client);
            var Component2 = new ComponentNode("Component2", client);
            var Component3 = new ComponentNode("Component3", client);
            var Component4 = new ComponentNode("Component4", client);
            Console.WriteLine("Components Created");

            // Create relationships...
            // ... for Widget A
            widgetA.AddComponent(Component1);
            widgetA.AddComponent(Component2);
            widgetA.AddComponent(Component3);
            // ... for Widget B
            widgetB.AddComponent(Component2);
            widgetB.AddComponent(Component3);
            widgetB.AddComponent(Component4);
            Console.WriteLine("Relationships Created\n");

            // Query - find components used in both widgets
            IEnumerable sharedComponents = WidgetNode.GetSharedComponents(widgetA, widgetB, client);
            if (sharedComponents.GetEnumerator().MoveNext() == false)
            {
                Console.WriteLine("No shared components identified");
            }
            else
            {
                Console.WriteLine("The following components are used by both {0} and {1}:\n", widgetA.Name, widgetB.Name);
                foreach (string c in sharedComponents)
                {
                    Console.WriteLine(c);
                }
            }
            Console.WriteLine();
        
        }

        static void ClearDown(GraphClient client)
        {
            // Deletes everything (assuming that all nodes have some usage relationship)
            client.Cypher
                .Match("(w:Widget)-[r:USED_IN]-(c:Component)")
                .Delete("r, w, c")
                .ExecuteWithoutResults();
        }

    }
}
