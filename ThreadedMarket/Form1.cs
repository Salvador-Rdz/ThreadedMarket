using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Collections;

namespace ThreadedMarket
{

    public partial class Form1 : Form
    {
        public bool isRunning = false;
        public MarketStall apples;
        public MarketStall oranges;
        public MarketStall grapes;
        public MarketStall watermelons;
        Thread appleStallP;
        Thread appleStallC;
        Thread orangeStallP;
        Thread orangeStallC;
        Thread grapeStallP;
        Thread grapeStallC;
        Thread watermelonStallP;
        Thread watermelonStallC;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false; //This handy line helps us access resources inbetween threads.
            timer1.Interval = 500;
        }

        private void startBt_Click(object sender, EventArgs e)
        {
            startBt.Enabled = false;
            endBT.Enabled = true;
            apples = new MarketStall(0, "apple", textBox1, 6000, 4000,10);
            oranges = new MarketStall(2, "orange", textBox2, 4000, 1000,16);
            grapes = new MarketStall(3, "grape", textBox4, 2000, 5000,20);
            watermelons = new MarketStall(5, "watermelon", textBox3, 8000, 10000,8);
            Thread.Sleep(3000);
            appleStallP = new Thread(new ThreadStart(apples.produce));
            appleStallC = new Thread(new ThreadStart(apples.consume));
            orangeStallP = new Thread(new ThreadStart(oranges.produce));
            orangeStallC = new Thread(new ThreadStart(oranges.consume));
            grapeStallP = new Thread(new ThreadStart(grapes.produce));
            grapeStallC = new Thread(new ThreadStart(grapes.consume));
            watermelonStallP = new Thread(new ThreadStart(watermelons.produce));
            watermelonStallC = new Thread(new ThreadStart(watermelons.consume));
            appleStallP.Start();
            appleStallC.Start();
            orangeStallP.Start();
            orangeStallC.Start();
            grapeStallP.Start();
            grapeStallC.Start();
            watermelonStallP.Start();
            watermelonStallC.Start();
            isRunning = true;
            timer1.Start();
        }

        private void endBT_Click(object sender, EventArgs e)
        {
            isRunning = false;
            startBt.Enabled = true;
            endBT.Enabled = false;
            appleStallP.Abort();
            appleStallC.Abort();
            orangeStallP.Abort();
            orangeStallC.Abort();
            grapeStallP.Abort();
            grapeStallC.Abort();
            watermelonStallP.Abort();
            watermelonStallC.Abort();
            textBox1.AppendText("\r\nThe Apple Stand has closed");
            textBox2.AppendText("\r\nThe Orange Stand has closed");
            textBox4.AppendText("\r\nThe Grape Stand has closed");
            textBox3.AppendText("\r\nThe Watermelon Stand has closed");
        }
        public class MarketStall
        {
            private  Semaphore fillCount;
            private  Semaphore emptyCount;
            private  Mutex mut = new Mutex();
            public ArrayList fruits;
            int nFruits=0;
            int maxFruits = 0;
            int count = 0;
            int prWait = 0;
            int coWait = 0;
            bool isRunning=false;
            string tFruit="undefined";
            TextBox tBox;
            public MarketStall(int nFruits, string tFruit, TextBox tBox, int prWait, int coWait, int maxFruits)
            {
                this.nFruits = nFruits;
                this.tFruit = tFruit;
                this.tBox = tBox;
                this.prWait = prWait;
                this.coWait = coWait;
                this.maxFruits = maxFruits;
                fruits = new ArrayList(maxFruits);
                for (int i =1;i<=nFruits;i++)
                {
                    fruits.Add("" + tFruit + "" + i);
                    count++;
                }
                fillCount = new Semaphore(maxFruits-nFruits, maxFruits);
                emptyCount = new Semaphore(nFruits, maxFruits);
                tBox.Text = "Fruit market stall has started with " + nFruits + " " + tFruit + "s";
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            private bool isFull()
            {
                return fruits.Capacity== this.nFruits;
            }

            private bool isEmpty()
            {
                return fruits.Count == 0;
            }

            public void produce()
            {
                while (true)
                {
                    if (isFull())
                    {
                        tBox.AppendText("\r\n" + tFruit + " stall is full, Farmer is waiting");
                        Thread.Sleep(coWait);
                        Thread.Yield();
                    }
                        mut.WaitOne();
                        fillCount.WaitOne();
                        count++;
                        string fruit = "" + tFruit + "" + count;
                        fruits.Add(fruit); //Critical area
                        nFruits++;
                        tBox.AppendText("\r\n" + "Farmer has produced " + fruit);
                        emptyCount.Release(1);
                        mut.ReleaseMutex();
                        int rWait = new Random().Next(1000, prWait);
                        Thread.Sleep(rWait);
                }
            }

            public void consume()
            {
                while (true)
                {
                    if (isEmpty())
                    {
                        tBox.AppendText( "\r\n" + tFruit + " stall is empty, customer is waiting");
                        Thread.Sleep(prWait);
                        Thread.Yield();
                    }
                    emptyCount.WaitOne();
                    mut.WaitOne();
                    string fruit = fruits[0].ToString();
                    fruits.RemoveAt(0);
                    nFruits--;
                    mut.ReleaseMutex();
                    fillCount.Release(1);
                    tBox.AppendText("\r\nClient has purchased " + fruit);
                    int rWait = new Random().Next(1000, coWait);
                    Thread.Sleep(rWait);
                }
            }
            public int getFruitCount()
            {
                return nFruits;
            }
            public int getMaxFruits()
            {
                return maxFruits;
            }
            public void setRunning(bool isRunning)
            {
                this.isRunning = isRunning;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(isRunning)
            {
                
                label5.Text = "" + apples.getFruitCount() + "/"+apples.getMaxFruits();
                label6.Text = "" + oranges.getFruitCount() + "/"+oranges.getMaxFruits();
                label7.Text = "" + grapes.getFruitCount() + "/"+grapes.getMaxFruits();
                label8.Text = "" + watermelons.getFruitCount() + "/"+watermelons.getMaxFruits();
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
}
