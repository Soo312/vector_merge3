using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace vector_merge3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, System.ComponentModel.INotifyPropertyChanged
    {
        private ObservableCollection<string> _listviewItemSource;

        List<List<VectorData>> totalData = new List<List<VectorData>>();

        string OpenPrjPath;

        List<LineString> special_word = new System.Collections.Generic.List<LineString>();

        public event PropertyChangedEventHandler PropertyChanged;

        bool ischangetxt = false;

        char change_bf_char = ' ', change_af_char = ' ';

        ProgressWindow proWnd;

        public System.Collections.ObjectModel.ObservableCollection<string> listviewItemSource
        {
            get
            {
                return _listviewItemSource;
            }
            set
            {
                if (value != null && value != listviewItemSource)
                    _listviewItemSource = value;
            }
        }

        public class VectorData
        {
            public string vec_name;


            public List<Vec_data_data> vec_data = new List<Vec_data_data>();

            public void Setvecname(string astring)
            {
                vec_name = astring;
            }

        }

        public class Vec_data_data
        {
            public char data;
            public Vec_data_data()
            {

            }

            public Vec_data_data(int count, char data)
            {
                this.data = data;
            }
        }



        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public MainWindow()
        {
            OpenPrjPath = "";
            listviewItemSource = new ObservableCollection<string>();
            InitializeComponent();

            DataContext = this;

        }



        private void btn_loadProject_Click(object sender, RoutedEventArgs e)
        {

            List<int> removeidx = new List<int>();

            CommonOpenFileDialog cofd = new CommonOpenFileDialog();
            //cofd.IsFolderPicker = true;

            totalData.Clear();

            cofd.Multiselect = true;
            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                //OpenPrjPath = cofd.FileNames;
                Logoutput("[" + DateTime.Now.ToString("yyMMdd_HHmmss") + "]  " + "Directory Open");

                foreach (string s in cofd.FileNames)
                {
                    listviewItemSource.Add(s);
                }
                OnPropertyChanged("listviewItemSource");


            }

            int sqpg_cnt = 0;
            for (int pj_cnt = 0; pj_cnt < listviewItemSource.Count; pj_cnt++)
            {
                //List<string> liststr = new List<string>();
                StreamReader SR = new StreamReader(listviewItemSource[pj_cnt].ToString());

                List<VectorData> list_vec = new List<VectorData>();



                string line;

                int cnt = 1;


                try
                {


                    while (true)
                    {

                        //test 한파일 전부다읽기
                        if ((line = SR.ReadLine()) != null)
                        {

                            if (line.Length > 0)
                            {
                                string[] strarr = line.Split(' ');
                                if (strarr[0].Equals("FORMAT"))
                                {
                                    //이름 시작점은 FORMAT이었음
                                    for (int i = 0; i < strarr.Count(); i++)
                                    {
                                        if (strarr[i].Equals("FORMAT") || strarr[i].Equals(";"))
                                            continue;
                                        VectorData avecd = new VectorData();
                                        avecd.Setvecname(strarr[i]);

                                        list_vec.Add(avecd);
                                    }
                                    continue;
                                }
                                else
                                {

                                    string[] stra = line.Split(' ');

                                    stra = stra.Where(str => str != "").ToArray();

                                    //중간에 공백이 하나 더있는 경우
                                    //if (stra[0].Equals("SQPG"))
                                    if (Array.IndexOf(stra, "SQPG") >= 0)
                                    {
                                        //SQPG 명령어 일 경우 루프 or 패딩 표현해야함
                                        if (sqpg_cnt > 0)
                                            sqpg_cnt--;

                                        if (Array.IndexOf(stra, "RPTV") >= 0)
                                        {
                                            special_word.Add(new LineString(
                                                sqpg_cnt
                                                , "S \t" + stra[stra.Length - 1].ToString()));
                                        }
                                        else if (Array.IndexOf(stra, "PADDING") >= 0 || Array.IndexOf(stra, "PADDING;") >= 0)
                                        {
                                            special_word.Add(new LineString(sqpg_cnt, "end"));
                                        }
                                    }
                                    else if (stra[0].Equals("R1"))
                                    {

                                        int forcnt = 0;

                                        string temp_str = "";

                                        for (int i2 = 0; i2 < stra.Length; i2++)
                                        {
                                            if (stra[i2].Length == list_vec.Count())
                                            {
                                                //데이터 영역
                                                int fori = 0;
                                                foreach (char achar in stra[i2])
                                                {
                                                    list_vec[fori].vec_data.Add(new Vec_data_data(fori, achar));

                                                    fori++;
                                                }
                                                break;
                                            }
                                            else
                                            {

                                                if (i2 >= 2)
                                                {

                                                    if (stra[i2].IndexOf(";") == 0)
                                                    {
                                                        //블링크후 바로";" 일 경우 사용안함
                                                    }
                                                    else
                                                    {
                                                        if (i2 == stra.Length - 1)
                                                            temp_str += stra[i2];
                                                        else
                                                        {
                                                            temp_str += stra[i2] + " ";
                                                            removeidx.Add(temp_str.Length - 1);
                                                        }
                                                    }

                                                    if (i2 == stra.Length - 1)
                                                    {
                                                        int fori = 0;
                                                        foreach (char achar in temp_str)
                                                        {
                                                            list_vec[fori].vec_data.Add(new Vec_data_data(fori, achar));

                                                            fori++;
                                                        }
                                                    }
                                                }

                                            }

                                            forcnt++;
                                        }

                                    }


                                }
                            }
                            sqpg_cnt++;
                        }
                        else
                            break;

                        cnt++;
                    }

                    removeidx = removeidx.Distinct().ToList();

                    foreach (int rmidx in removeidx.ToArray().Reverse())
                    {
                        list_vec.RemoveAt(rmidx);
                    }

                    totalData.Add(list_vec);

                    SR.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "Data Read Error");
                }
            }

        }

        private void Make_and_Save()
        {
            try
            {
                int totalCount = 0;

                for (int i = 0; i < totalData.Count; i++)
                {
                    totalCount = totalData[i][0].vec_data.Count;
                }

                if (listviewItemSource.Count > 0)
                {

                    List<VectorData> mergeVeclist = new List<VectorData>();
                    List<string> samenamelist = new List<string>();
                    for (int i = 0; i < totalData.Count; i++)
                    {
                        if (mergeVeclist.Count == 0)
                        {
                            //mergeVeclist = totalData[0].ToList();
                            mergeVeclist = totalData[0].ConvertAll(o => new VectorData { vec_data = o.vec_data.ConvertAll(s => new Vec_data_data { data = s.data }), vec_name = o.vec_name });
                            continue;
                        }
                        else
                        {
                            //i==1 부터 실제로 비교 추가

                            bool issame = false;

                            List<VectorData> bk_mergedata = mergeVeclist.ToList();
                            for (int k = 0; k < totalData[i].Count; k++)
                            {
                                for (int j = 0; j < mergeVeclist.Count; j++)
                                {
                                    try
                                    {

                                        if (mergeVeclist[j].vec_name.Equals(totalData[i][k].vec_name))
                                        {
                                            issame = true;
                                            if (samenamelist.IndexOf(totalData[i][k].vec_name) > 0)
                                            {

                                            }
                                            else
                                            {
                                                samenamelist.Add(totalData[i][k].vec_name);

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message + "i==1 부터 실제로 비교 추가" + i + "/" + j + "/" + k);
                                    }
                                }


                            }

                        }

                        //이름 결합과 데이터 결합 분리
                        for (int k = 0; k < totalData[i].Count; k++)
                        {
                            try
                            {
                                if (samenamelist.IndexOf(totalData[i][k].vec_name) >= 0)
                                {

                                }
                                else
                                {
                                    VectorData anoewvecData = new VectorData();
                                    anoewvecData.vec_name = totalData[i][k].vec_name;

                                    // 빈칸채우기
                                    for (int l = 0; l < totalData[0][0].vec_data.Count; l++)
                                    {
                                        anoewvecData.vec_data.Add(new Vec_data_data(l, 'X'));
                                    }

                                    mergeVeclist.Add(anoewvecData);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message + "이름 결합과 데이터 결합 분리" + i + "/" + " " + "/" + k);
                            }
                            //위에코드는 이전꺼 vec_data채우기+ 새로vec_name만들기
                            //밑에껀 추가한 vec_data추가하기
                        }

                    }


                    for (int i = 1; i < totalData.Count; i++)
                    {

                        //2번을 mergeVec에 추가한다
                        //스탭1. 추가할 데이터를 선별한다
                        if (totalData[i] != null)
                        {
                            for (int k = 0; k < mergeVeclist.Count; k++)
                            //for (int j = 0; j < totalData[i].Count; j++) //선별할 대이터의 column갯수
                            {
                                bool is_same = false;
                                for (int j = 0; j < totalData[i].Count; j++) //선별할 대이터의 column갯수
                                                                             //for (int k = 0; k < mergeVeclist.Count; k++)
                                {
                                    try
                                    {
                                        if (mergeVeclist[k].vec_name.Equals(totalData[i][j].vec_name))
                                        {
                                            //선별한 데이터의 vec_name과 mergeVeclist[k].vec_name 의 이름이 같은 상황


                                            int d_cnt = 0;
                                            foreach (Vec_data_data avdd in totalData[i][j].vec_data)
                                            {

                                                mergeVeclist[k].vec_data.Add(avdd);//같은이름 정상 추가
                                                d_cnt++;
                                            }

                                            is_same = true;
                                            continue;//찾았으니까 뒤에 비교는 필요없어서 스킵
                                        }
                                        else
                                        {
                                            //다른상황 or 찾지 못한 상황을 구별해야함
                                            if (j == totalData[i].Count - 1 && is_same == false)//is_same 을 추가함으로써 뒤에추가되는 XXX 삭제
                                            {
                                                //if (k != 0)
                                                {
                                                    int d_cnt = 0;
                                                    //아마 for문 끝까지 찾지못한 상황 == 다른상황 현 totalData[i]에는 없는것으로 추측될 경우
                                                    foreach (Vec_data_data avdd in totalData[i][j].vec_data)
                                                    {

                                                        mergeVeclist[k].vec_data.Add(new Vec_data_data(d_cnt, 'X'));//같은이름 정상 추
                                                        d_cnt++;
                                                    }
                                                }

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("추가할 데이터를 선별한다+MergeVec에 저장" + i + "/" + j + "/" + k);
                                    }
                                }
                            }
                        }
                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                        {
                            proWnd.setProgressBar((int)(((double)i / (double)totalData.Count) * (double)100), "File Reading");
                        }));

                        totalData[i].Clear();

                        Thread.Sleep(1);
                    }

                    mergeVeclist.Sort((name1, name2) => name1.vec_name.CompareTo(name2.vec_name));

#if true //stringbuilder사용

                    StringBuilder outputstring = new StringBuilder();
                    int size_maxLen = 0;
                    for (int i = 0; i < mergeVeclist.Count; i++)
                    {
                        if (mergeVeclist[i].vec_name.Length > size_maxLen)
                            size_maxLen = mergeVeclist[i].vec_name.Length;
                        //최대이름크기 찾기
                    }

                    string savepath = @"E:\Work_2022_05_18after\\output\\" + "fix.test";

                    //이름 세로로 만들어 넣기
                    for (int j = 0; j < size_maxLen; j++)
                    {
                        for (int i = 0; i < mergeVeclist.Count; i++)
                        {
                            if (i == 0)
                            {
                                //명령어 공백을 만들기 위한 빈칸추가
                                outputstring.Append("               ");
                            }
                            try
                            {
                                outputstring.Append(mergeVeclist[i].vec_name[j] + " ");
                            }
                            catch (Exception ex)
                            {
                                outputstring.Append("  ");
                            }
                        }
                        outputstring.Append("\n");

                    }


                    int step_cnt = 0;

                    int max_step = (mergeVeclist[0].vec_data.Count * mergeVeclist.Count);
                    //데이터 넣기
                    for (int j = 0; j < mergeVeclist[0].vec_data.Count; j++)
                    {
                        for (int sp = 0; sp < special_word.Count; sp++)
                        {
                            if (j == special_word[sp].line)
                            {
                                outputstring.Append(special_word[sp].linetext + "\n");
                                //outputstring.Append("   " + "\n");
                            }
                        }
                        for (int i = 0; i < mergeVeclist.Count; i++)
                        {
                            if (i == 0)
                            {
                                //명령어 공백을 만들기 위한 빈칸추가
                                outputstring.Append("               ");
                            }
                            char outputchar = mergeVeclist[i].vec_data[j].data;
                            try
                            {
                                if (ischangetxt)
                                {
                                    if (outputchar.Equals(change_bf_char))
                                    {
                                        outputchar = change_af_char;
                                    }
                                }

                                outputstring.Append(outputchar + " ");
                            }
                            catch (Exception ex)
                            {
                                // outputstring += "  ";
                                outputstring.Append("  ");
                            }
                            step_cnt++;

                        }
                        outputstring.Append("\n");
                        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                        {
                            proWnd.setProgressBar((int)((double)step_cnt / (double)max_step * (double)100),
                                step_cnt.ToString() + " / " + max_step.ToString());
                        }));
                    }

                    //하나출력
                    //System.IO.File.WriteAllText(OpenPrjPath + "fix", outputstring.ToString());
                    Thread.Sleep(10);
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        proWnd.setProgressBar(100,
                            "Done");
                    }));

                    string outputname = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    System.IO.File.WriteAllText(outputname + ".txt", outputstring.ToString());


                    mergeVeclist.Clear();//메모리때문에 빨리지워야함
                    System.GC.Collect();



#else
            string outputstring = "";
            int size_maxLen = 0;
            for(int i=0; i<list_vec.Count; i++)
            {
                if(list_vec[i].vec_name.Length > size_maxLen)
                    size_maxLen = list_vec[i].vec_name.Length;
                //최대이름크기 찾기
            }


            //이름 세로로 만들어 넣기
            for (int j = 0; j < size_maxLen; j++)
            {
                for (int i = 0; i < list_vec.Count; i++)
                {

                    try
                    {
                        outputstring += list_vec[i].vec_name[j] + " ";
                    }
                    catch (Exception ex)
                    {
                        outputstring += "  ";
                    }
                }
                outputstring += "\n";

            }

            //데이터 넣기
            for (int j = 0; j < list_vec[0].vec_data.Count; j++)
            {
                for (int i = 0; i < list_vec.Count; i++)
                {

                    try
                    {
                        outputstring += list_vec[i].vec_data[j].data + " ";
                    }
                    catch (Exception ex)
                    {
                        outputstring += "  ";
                    }
                }
                outputstring += "\n";

            }

            System.IO.File.WriteAllText(OpenPrjPath+"fix", outputstring);
#endif

                }
            }
            catch (ThreadInterruptedException e)
            {
                Environment.Exit(0);
            }
        }

        private void btn_textchange_Click(object sender, RoutedEventArgs e)
        {



            if (btn_textchange.Content.Equals("적용"))
            {
                btn_textchange.Content = "적용중";
                btn_textchange.Background = Brushes.OrangeRed;
                ischangetxt = true;
                change_bf_char = tb_change_bf.Text[0];
                change_af_char = tb_change_af.Text[0];

                Logoutput("[" + DateTime.Now.ToString("yyMMdd_HHmmss") + "]  " + "set_textChange");
            }
            else
            {
                btn_textchange.Content = "적용";
                btn_textchange.Background = Brushes.LightGreen;
                ischangetxt = false;
                Logoutput("[" + DateTime.Now.ToString("yyMMdd_HHmmss") + "]  " + "unset_textChange");
            }


        }

        private void Logoutput(string str)
        {
            if (tb_log.Text != null)
                tb_log.Text += "\n";
            tb_log.Text += str;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Thread mkdata_and_save = new Thread(() => Make_and_Save());
            mkdata_and_save.Start();

            mkdata_and_save.IsBackground = false;

            proWnd = new ProgressWindow();

            if (proWnd.ShowDialog() ?? false)
            {

            }
            else
            {
                //mkdata_and_save.Interrupt();
                mkdata_and_save.Abort();

            }
            Logoutput("[" + DateTime.Now.ToString("yyMMdd_HHmmss") + "]  " + "Save Done");
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            _listviewItemSource.Clear();

            totalData = new List<List<VectorData>>();

            OpenPrjPath = "";

            special_word.Clear();

            proWnd = new ProgressWindow();

            System.GC.Collect();
        }
    }

}
