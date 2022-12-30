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

        List<STRSTR> pinlist = new List<STRSTR>(); // Pin파일로 정렬하는용

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

        public struct VectorData
        {
            public string vec_name;


            public List<Vec_data_data> vec_data;



        }

        public struct Vec_data_data
        {
            public char data;
            //public Vec_data_data()
            //{

            //}

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
                                        avecd.vec_name = (strarr[i]);
                                        avecd.vec_data = new List<Vec_data_data>();

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
                GC.Collect();
            }

        }

        private void Make_and_Save()
        {
            try
            {


                int totalCount = 0;

                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    proWnd.setProgressBar("File Loading...");
                }));

                List<string> samenamelist = new List<string>();

                for (int i = 0; i < totalData.Count; i++)
                {
                    totalCount = totalData[i][0].vec_data.Count;
                    if (i == 0)
                    {
                        for (int j = 0; j < totalData[i].Count; j++)
                        {
                            samenamelist.Add(totalData[i][j].vec_name);
                        }
                    }


                }

                if (listviewItemSource.Count > 0)
                {

                    List<VectorData> mergeVeclist = new List<VectorData>();

                    //for(int i = 1; i< totalData.Count;i++)
                    //{
                    //    //totalData[0][j].vec_data 에다 일단 빈'X'를 다 추가할 계획 길이 초기화
                    //    for (int j = 0; j < totalData[i].Count; j++)
                    //    {
                    //        if (j < totalData[0].Count)
                    //        {
                    //            for (int k = 0; k < totalData[i][j].vec_data.Count; k++)
                    //            {
                    //                totalData[0][j].vec_data.Add(new Vec_data_data(0, 'X'));
                    //            }
                    //        }
                    //    }
                    //}

                    for (int i = 1; i < totalData.Count; i++)
                    {
                        int max_data_cnt = 0;

                        for (int j = 0; j < totalData[i].Count; j++)
                        {

                            if (samenamelist.IndexOf(totalData[i][j].vec_name) >= 0)
                            {
                                //이름 겹침
                                foreach (Vec_data_data avdd in totalData[i][j].vec_data)
                                {
                                    totalData[0][samenamelist.IndexOf(totalData[i][j].vec_name)].vec_data.Add(avdd);
                                }

                                if (max_data_cnt < totalData[0][samenamelist.IndexOf(totalData[i][j].vec_name)].vec_data.Count)
                                    max_data_cnt = totalData[0][samenamelist.IndexOf(totalData[i][j].vec_name)].vec_data.Count;
                            }
                            else
                            {
                                samenamelist.Add(totalData[i][j].vec_name);
                                VectorData avd = new VectorData();
                                avd.vec_name = (totalData[i][j].vec_name);
                                avd.vec_data = new List<Vec_data_data>();

                                for (int k = 0; k < totalData[0][0].vec_data.Count; k++)
                                {
                                    avd.vec_data.Add(new Vec_data_data(0, 'X'));
                                }

                                foreach (Vec_data_data avdd in totalData[i][j].vec_data)
                                {
                                    avd.vec_data.Add(avdd);
                                }

                                totalData[0].Add(avd);

                                if (max_data_cnt < totalData[0][totalData[0].Count - 1].vec_data.Count)
                                    max_data_cnt = totalData[0][totalData[0].Count - 1].vec_data.Count;
                            }

                            
                            switch()
                            {
                                case 0:

                                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                                    {
                                        proWnd.setProgressBar("File Loading.");
                                    }));
                                    break;
                                case 1:

                                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                                    {
                                        proWnd.setProgressBar("File Loading..");
                                    }));
                                    break;
                                case 2:

                                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                                    {
                                        proWnd.setProgressBar("File Loading...");
                                    }));
                                    break;
                                default:break;
                            }
                            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                            {
                                proWnd.setProgressBar("File Loading...");
                            }));
                        }
                        //totalData[i].Clear();

                        for (int f = 0; f < totalData[0].Count; f++)
                        {
                            if (totalData[0][f].vec_data.Count < max_data_cnt)
                            {
                                for (int j = 0; j < max_data_cnt - totalData[0][f].vec_data.Count;)
                                {
                                    totalData[0][f].vec_data.Add(new Vec_data_data(0, 'X'));
                                }
                            }
                            else if (totalData[0][f].vec_data.Count > max_data_cnt)
                            {
                                //있어선 안됨
                            }
                            else
                            {
                                //정상
                            }
                        }

                    }

                    totalData[0].Sort((name1, name2) => name1.vec_name.CompareTo(name2.vec_name));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("파일 정렬 " + ex.ToString());
            }

#if true //stringbuilder사용
            try
            {
                StringBuilder outputstring = new StringBuilder();
                int size_maxLen = 0;
                for (int i = 0; i < totalData[0].Count; i++)
                {
                    if (totalData[0][i].vec_name.Length > size_maxLen)
                        size_maxLen = totalData[0][i].vec_name.Length;
                    //최대이름크기 찾기
                }

                string savepath = @"E:\Work_2022_05_18after\\output\\" + "fix.test";

                //이름 세로로 만들어 넣기
                for (int j = 0; j < size_maxLen; j++)
                {
                    for (int i = 0; i < totalData[0].Count; i++)
                    {
                        if (i == 0)
                        {
                            //명령어 공백을 만들기 위한 빈칸추가
                            outputstring.Append("               ");
                        }
                        try
                        {
                            //outputstring.Append(totalData[0][i].vec_name[j]);// + " "); 빈칸 삭제 221221
                            outputstring.Append(totalData[0][i].vec_name[j]); 
                        }
                        catch (Exception ex)
                        {
                            outputstring.Append(" ");
                        }
                    }
                    outputstring.Append("\n");

                }


                int step_cnt = 0;

                int max_step = (totalData[0][0].vec_data.Count * totalData[0].Count);

                string outputname = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                System.IO.File.WriteAllText(outputname + ".txt", outputstring.ToString());

                outputstring = new StringBuilder();

                //데이터 넣기

                int writecount = 0;

                for (int j = 0; j < totalData[0][0].vec_data.Count; j++)
                {
                    for (int sp = 0; sp < special_word.Count; sp++)
                    {
                        if (j == special_word[sp].line)
                        {
                            outputstring.Append(special_word[sp].linetext + "\n");
                            //outputstring.Append("   " + "\n");
                        }
                    }
                    for (int i = 0; i < totalData[0].Count; i++)
                    {
                        if (i == 0)
                        {
                            //명령어 공백을 만들기 위한 빈칸추가
                            outputstring.Append("               ");
                        }
                        char outputchar = totalData[0][i].vec_data[j].data;
                        try
                        {
                            if (ischangetxt)
                            {
                                if (outputchar.Equals(change_bf_char))
                                {
                                    outputchar = change_af_char;
                                }
                            }

                            //outputstring.Append(outputchar);// + " "); 221221 빈칸삭제

                            outputstring.Append(outputchar);
                        }
                        catch (Exception ex)
                        {
                            // outputstring += "  ";
                            outputstring.Append(" ");//("  ");빈칸 삭제
                        }
                        step_cnt++;

                    }
                    outputstring.Append("\n");
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    {
                        proWnd.setProgressBar((int)((double)step_cnt / (double)max_step * (double)100),
                            step_cnt.ToString() + " / " + max_step.ToString());
                    }));

                    if (writecount % 10000 == 0 || writecount == totalData[0][0].vec_data.Count - 1)
                    {
                        System.IO.File.AppendAllText(outputname + ".txt", outputstring.ToString());
                        outputstring.Clear();

                    }
                    writecount++;
                }

                //하나출력
                //System.IO.File.WriteAllText(OpenPrjPath + "fix", outputstring.ToString());
                Thread.Sleep(10);
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                {
                    proWnd.setProgressBar(100,
                        "Done");
                }));



                //mergeVeclist.Clear();//메모리때문에 빨리지워야함
                System.GC.Collect();

            }
            catch (Exception ex)
            {
                MessageBox.Show("저장 " + ex.ToString());
            }

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

        bool first_doing = false;
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Thread mkdata_and_save = new Thread(() => Make_and_Save());

            first_doing = true;
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

            //btn_clear_Click(null, null);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cofd = new CommonOpenFileDialog();


            cofd.Multiselect = false;

            if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                //OpenPrjPath = cofd.FileNames;
                Logoutput("[" + DateTime.Now.ToString("yyMMdd_HHmmss") + "]  " + "TestButton On");

                string pinpath = cofd.FileName;

                if(pinpath != null)
                {
                    StreamReader streamreader = new StreamReader(pinpath);

                    string pinstr = streamreader.ReadToEnd();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string[] strarr = pinstr.Split(stringSeparators, StringSplitOptions.None);



                    for (int i = 0; i < strarr.Count(); i++)
                    {
                        strarr[i] = strarr[i].Trim();
                        STRSTR apinname = new STRSTR();

                        string[] partsstr = strarr[i].Split(' ');

                        if (string.IsNullOrWhiteSpace(partsstr[0]))
                        {
                            continue;
                        }
                        else
                        {
                            apinname.str_number = partsstr[0];
                            apinname.str_name = partsstr[partsstr.Length - 1];
                        }

                        pinlist.Add(apinname);

                    }

                    List<VectorData> pinsortData = new List<VectorData>();

                    for (int i=0;i<pinlist.Count; i++)
                    {
                        VectorData tempvecdata = new VectorData();

                        for(int j=0; j < totalData[0].Count; j++)
                        {
                            if (pinlist[i].str_name.Equals(totalData[0][j].vec_name))
                            {
                                //비교해서 같음
                                tempvecdata.vec_name = pinlist[i].str_name;

                                tempvecdata.vec_data = new List<Vec_data_data>();

                                for (int k = 0; k < totalData.Count; k++)
                                {
                                    tempvecdata.vec_data.AddRange(totalData[k][j].vec_data);
                                }
                                //한번이라도 같으면 동일한것 탈출
                                break;
                            }
                            else
                            {
                                //비교해서 다름
                                if (j == totalData[0].Count - 1)
                                {
                                    //끝까지 비교해도 다름? 같음?
                                    tempvecdata.vec_name = pinlist[i].str_name;

                                    tempvecdata.vec_data = new List<Vec_data_data>();

                                    int datalength = 0;
                                    for (int ta = 0; ta < totalData.Count; ta++)
                                    {
                                        datalength += totalData[ta][0].vec_data.Count;
                                    }
                                    for (int k = 0; k < datalength; k++)
                                    {
                                        Vec_data_data temp_vdd = new Vec_data_data();
                                        temp_vdd.data = '.';
                                        tempvecdata.vec_data.Add(temp_vdd);
                                    }
                                }
                            }

                        }

                        pinsortData.Add(tempvecdata);

                    }

                    string outputname = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                    StringBuilder outputstring = new StringBuilder();

                    System.IO.File.AppendAllText(outputname + ".pin", outputstring.ToString());


                    //이름 넣기
                    int size_maxLen = 0;
                    for (int i = 0; i < pinsortData.Count; i++)
                    {
                        if (pinsortData[i].vec_name.Length > size_maxLen)
                            size_maxLen = pinsortData[i].vec_name.Length;
                        //최대이름크기 찾기
                    }

                    //이름 세로로 만들어 넣기
                    for (int j = 0; j < size_maxLen; j++)
                    {
                        for (int i = 0; i < pinsortData.Count; i++)
                        {
                            if (i == 0)
                            {
                                //명령어 공백을 만들기 위한 빈칸추가
                                outputstring.Append("               ");
                            }
                            try
                            {
                                //outputstring.Append(totalData[0][i].vec_name[j]);// + " "); 빈칸 삭제 221221
                                outputstring.Append(pinsortData[i].vec_name[j]);
                            }
                            catch (Exception ex)
                            {
                                outputstring.Append(" ");
                            }
                        }
                        outputstring.Append("\n");
                    }

                    int step_cnt = 0;

                    int max_step = (pinsortData.Count);


                    System.IO.File.WriteAllText(outputname + ".pin", outputstring.ToString());

                    outputstring = new StringBuilder();

                    //데이터 넣기

                    int writecount = 0;

                    for (int j = 0; j < pinsortData[0].vec_data.Count; j++)
                    {
                        for (int sp = 0; sp < special_word.Count; sp++)
                        {
                            if (j == special_word[sp].line)
                            {
                                outputstring.Append(special_word[sp].linetext + "\n");
                                //outputstring.Append("   " + "\n");
                            }
                        }
                        for (int i = 0; i < pinsortData.Count; i++)
                        {
                            if (i == 0)
                            {
                                //명령어 공백을 만들기 위한 빈칸추가
                                outputstring.Append("               ");
                            }
                            char outputchar = pinsortData[i].vec_data[j].data;
                            try
                            {
                                if (ischangetxt)
                                {
                                    if (outputchar.Equals(change_bf_char))
                                    {
                                        outputchar = change_af_char;
                                    }
                                }

                                //outputstring.Append(outputchar);// + " "); 221221 빈칸삭제

                                outputstring.Append(outputchar);
                            }
                            catch (Exception ex)
                            {
                                // outputstring += "  ";
                                outputstring.Append(" ");//("  ");빈칸 삭제
                            }
                            step_cnt++;

                        }
                        outputstring.Append("\n");
                        //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                        //{
                        //     proWnd.setProgressBar((int)((double)step_cnt / (double)max_step * (double)100),
                        //        step_cnt.ToString() + " / " + max_step.ToString());
                        //}));

                        if (writecount % 10000 == 0 || writecount == totalData[0][0].vec_data.Count - 1)
                        {
                            System.IO.File.AppendAllText(outputname + ".pin", outputstring.ToString());
                            outputstring.Clear();

                        }
                        writecount++;
                    }

                    //하나출력
                    //System.IO.File.WriteAllText(OpenPrjPath + "fix", outputstring.ToString());
                    //Thread.Sleep(10);
                    //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                    //{
                    //    proWnd.setProgressBar(100,
                    //        "Done");
                    //}));


                }
            }
        }

        struct STRSTR
        {
            public string str_number;
            public string str_name;
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            //_listviewItemSource.Clear();

            //totalData = new List<List<VectorData>>();

            //OpenPrjPath = "";

            special_word.Clear();

            proWnd = new ProgressWindow();

            System.GC.Collect();
        }
    }

}
