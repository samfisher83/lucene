using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lucene.Net.Store;
//using Lucene.Net.Analysis.Standard.StandardAnalyzer;
using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System.IO;
using System.Diagnostics;


namespace WindowsFormsApplication1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {




            luceneExample();
            //
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
        }


        private static void luceneExample()
        {
            Lucene.Net.Store.Directory index = new RAMDirectory();
            StandardAnalyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);



            OpenFileDialog open = new OpenFileDialog();
            open.ShowDialog();
            List<string> questions = new List<string>();

            string line;
            using(StreamReader inHandle = new StreamReader(open.FileName)){
                while ((line = inHandle.ReadLine()) != null)
                {
                    questions.Add(line);
                }
            
            }

            
            IndexWriter w = new IndexWriter(index, analyzer);


            foreach (string item in questions)
            {
                addDoc(w, item);
            }


            //addDoc(w, "Lucene in Action");
            //addDoc(w, "Lucene for Dummies");
            //addDoc(w, "Managing Gigabytes");
            //addDoc(w, "The Art of Computer Science");
            w.Close();
            Stopwatch time = new Stopwatch();
            time.Start();
            for (int i = 0; i < 100; i++)
            {
                String querystr = questions[i];
                Query q = null;
                try
                {
                    q = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "title", analyzer).Parse(querystr);
                }
                catch (Exception)
                {
                    goto skip;
                }
                    //q.Parse();


                int hitsPerPage = 10;
                IndexReader reader = IndexReader.Open(index, true);
                IndexSearcher searcher = new IndexSearcher(reader);
                TopScoreDocCollector collector = TopScoreDocCollector.Create(hitsPerPage, true);
                searcher.Search(q, collector);
                ScoreDoc[] hits = collector.TopDocs().ScoreDocs;

                System.Console.WriteLine("Found {0} Hits", hits.Length);
                int j = 0;
                foreach (var item in hits)
                {
                    int docId = item.Doc;
                    Document d = searcher.Doc(docId);
                    System.Console.WriteLine(++j + " " + d.Get("title") + " " + item.Score);
                }

            skip:
                int k = 0;
            }
            time.Stop();
            System.Console.WriteLine(time.ElapsedMilliseconds);
 

        }

        private static void addDoc(IndexWriter w, String value)
        {
            Document doc = new Document();

            doc.Add(new Field("title", value, Field.Store.YES, Field.Index.ANALYZED));
            w.AddDocument(doc);
        }
    }
}
