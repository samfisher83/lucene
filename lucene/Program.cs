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
using System.Web;
using System.Web.Script.Serialization;


namespace WindowsFormsApplication1
{
    public class text_score {
        public string text1 = "";
        public float score1 = 0;
        public string text2 = "";
        public float score2 = 0;
        public string text3 = "";
        public float score3 = 0;
        public string text4 = "";
        public float score4 = 0;
        public string text5 = "";
        public float score5 = 0;
    }
    public class array_text_score
    {
        public text_score[] array_text_scores = new text_score[5];
    }
    
    
    
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
            List<text_score> text_scores = new List<text_score>();
            JavaScriptSerializer jsonIn = new JavaScriptSerializer();
            using (StreamReader inHandle = new StreamReader(open.FileName))
            {
                questions = jsonIn.Deserialize<List<string>>(inHandle.ReadToEnd());
         
            }


            
            open.ShowDialog();

            using (StreamReader inHandle = new StreamReader(open.FileName))
            {
                questions.AddRange(jsonIn.Deserialize<List<string>>(inHandle.ReadToEnd()));

            }

            
            //string line;
            //using(StreamReader inHandle = new StreamReader(open.FileName)){
            //    while ((line = inHandle.ReadLine()) != null)
            //    {
            //        questions.Add(line);
            //    }
            
            //}

            
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
            using (StreamWriter outhandle = new StreamWriter("out.txt"))
            {
                for (int i = 0; i < questions.Count; i++)
                {
                    System.Console.WriteLine("{0}/{1}", i, questions.Count);
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


                    int hitsPerPage = 5;
                    IndexReader reader = IndexReader.Open(index, true);
                    IndexSearcher searcher = new IndexSearcher(reader);
                    TopScoreDocCollector collector = TopScoreDocCollector.Create(hitsPerPage, true);
                    searcher.Search(q, collector);
                    ScoreDoc[] hits = collector.TopDocs().ScoreDocs;

                    //outhandle.Write("Found {0} Hits|", hits.Length);
                    text_score temp = new text_score();
                    int j = 0;
                    foreach (var item in hits)
                    {
                        int docId = item.Doc;
                        Document d = searcher.Doc(docId);
                        //temp.array_text_scores[j] = new text_score();
                        if (j == 0)
                        {
                            temp.text1 = d.Get("title");
                            temp.score1 = item.Score;
                        }else if (j == 1)
                        {
                            temp.text2 = d.Get("title");
                            temp.score2 = item.Score;
                        }
                        else if (j == 2)
                        {
                            temp.text3 = d.Get("title");
                            temp.score3 = item.Score;
                        }
                        else if (j == 3)
                        {
                            temp.text4 = d.Get("title");
                            temp.score4 = item.Score;
                        }
                        else if (j == 4)
                        {
                            temp.text5 = d.Get("title");
                            temp.score5 = item.Score;
                        }


                        
                        
                        
                        
                        //outhandle.Write( d.Get("title").Replace("\n"," ") + "|" + item.Score + "|");
                        j++;
                    }
                    text_scores.Add(temp);
                    outhandle.WriteLine();
                    

                skip:
                    int k = 0;
                }
                time.Stop();
                System.Console.WriteLine(time.ElapsedMilliseconds);
            }
            using (StreamWriter outh = new StreamWriter("out.xml"))
            {
                System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<text_score>));
                xml.Serialize(outh, text_scores);

            }

        }

        private static void addDoc(IndexWriter w, String value)
        {
            Document doc = new Document();

            doc.Add(new Field("title", value, Field.Store.YES, Field.Index.ANALYZED));
            w.AddDocument(doc);
        }
    }
}
