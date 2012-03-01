using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Lucene.Net.Store;
//using Lucene.Net.Analysis.Standard.StandardAnalyzer;
using Lucene.Net.Documents;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;




namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Directory index = new RAMDirectory();
            StandardAnalyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);

            IndexWriter w = new IndexWriter(index, analyzer);


            addDoc(w, "Lucene in Action");
            addDoc(w, "Lucene for Dummies");
            addDoc(w, "Managing Gigabytes");
            addDoc(w, "The Art of Computer Science");
            w.Close();

            String querystr = "Lucene in Action";

            Query q =  new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "title", analyzer).Parse(querystr);
            //q.Parse();


            int hitsPerPage = 10;
            IndexReader reader = IndexReader.Open(index,true);
            IndexSearcher searcher = new IndexSearcher(reader);
            TopScoreDocCollector collector = TopScoreDocCollector.Create(hitsPerPage, true);
            searcher.Search(q, collector);
            ScoreDoc[] hits = collector.TopDocs().ScoreDocs;

            System.Console.WriteLine("Found {0} Hits", hits.Length);

            foreach (var item in hits)
            {
                int docId = item.Doc;
                Document d = searcher.Doc(docId);
                System.Console.WriteLine(d.Get("title") + " " + item.Score);
            }
            
        }

        private static void addDoc(IndexWriter w, String value) {
            Document doc = new Document();
            
              doc.Add(new Field("title", value, Field.Store.YES, Field.Index.ANALYZED));
              w.AddDocument(doc);
          }
    }
}
