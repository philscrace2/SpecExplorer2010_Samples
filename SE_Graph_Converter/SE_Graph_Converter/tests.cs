using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl;
using NUnit.Framework;
using System.Reflection;

namespace SE_Graph_Converter
{
    [TestFixture]
    public class tests
    {        
        [Test]
        public void Test1()
        {
            string path_to_graph = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "msagle_version_3.msagl");
            //FileStream fs = new FileStream(path_to_graph, FileMode.Open);           
            //GeometryGraphReader gr = new GeometryGraphReader(fs);
            //GeometryGraph msagl = gr.Read();

            Graph msagl = Graph.Read(path_to_graph);
            Console.WriteLine("Node count is {0}", msagl.NodeCount);

            
        }
    }
}
