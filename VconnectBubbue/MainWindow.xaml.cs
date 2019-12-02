using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Net;
using System.Net.Http;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using System.Collections.Specialized;
using Microsoft.Win32;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace VconnectBubbue
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        string site_url = "https://www.vconnect.com/";//holds site_url as mentioned
        string site_api = "https://nvn9agahxy-1.algolianet.com/1/indexes/BusinessSearch/query?x-algolia-agent=Algolia%20for%20vanilla%20JavaScript%20(lite)%203.29.0&x-algolia-application-id=NVN9AGAHXY&x-algolia-api-key=8e913380c293e0a4f0e3b0b017510b11";
        ObservableCollection<LaundryCompany> myEmails = new ObservableCollection<LaundryCompany>();//email addr
        OpenFileDialog dlg;//browse file to append
        string filename;//hold file name
        string responseString;
        string search_url;
        int totalProgress = 0;
        bool chkbx = false;
        string locationstr = "Lagos";
        int pageno = 0;
        Thread th;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void searchbtn_Click(object sender, RoutedEventArgs e)
        {
            locationstr = locationtxt.Text;
            
            myEmails.Clear();
            vconnectDG.ItemsSource = myEmails;
            //generate the search url
            if (!int.TryParse(pagetxt.Text,out pageno))
            {
                pageno = 0;
            }
            

            //auto_append_chkbx.IsEnabled = false;
            progressbar.IsEnabled = true;//progress starts simulating work done
            progressbar.Visibility = Visibility.Visible;//progressbar visible
            searchbtn.IsEnabled = false;//disable the search button until the work is done
            locationtxt.IsEnabled = false;
            pagetxt.IsEnabled = false;

            //Start the thread
            th = new Thread(searchEngine);
            th.Start();
            
        }

        
        private void searchEngine()
        {
            //Update the progress bar
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { progressbar.Value = 10; });
            String responseString;//The string variable holds the html page returned
            //bool isappending = true;//for stopping a loop when done appending

            
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                total_progressbar.Visibility = Visibility.Visible;
                savefile_btn.IsEnabled = true;
                //total_progressbar.Value = 5;
            });
            try
            {
                using (var client = new WebClient())
                {
                    //responseString = client.DownloadString(@search_url);//fetch html page
                    client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    client.Headers.Add("Accept", "application/json");
                    client.Headers.Add("Origin", "https://m.vconnect.com");
                    client.Headers.Add("Referer", "https://m.vconnect.com/qsearch?sq=Laundry&sl=Lagos&ref=search&page=1");
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Mobile Safari/537.36");
                    NameValueCollection postData = new NameValueCollection() 
                    { 
                            //{ "params", "query=Laundry&page=0&hitsPerPage=9&facets=*&maxValuesPerFacet=5&facetFilters=%5B%5B%22state%3ALagos%22%5D%2C%5B%5D%5D&aroundLatLngViaIP=false&aroundLatLng=&attributesToRetrieve=%5B%22businessid%22%2C%22description%22%2C%22businessurl%22%2C%22companylogo200%22%2C%22avgrating%22%2C%22topcategory%22%2C%22reviewcount%22%2C%22Likes%22%2C%22status%22%2C%22state%22%2C%22city%22%2C%22area%22%2C%22businessname%22%2C%22InsuredBadge%22%2C%22Mastercard%22%2C%22NewBadge%22%2C%22XpertBadge%22%2C%22TrustBadge%22%2C%22YearOnBadge%22%2C%22visitcount%22%2C%22hired%22%2C%22respond%22%2C%22lastactivedate%22%5D" },  //order: {"parameter name", "parameter value"}
                             //order: {"parameter name", "parameter value"}
                             {"params", ""}
                            
                    };
                    string pData = "{\"params\":\"query=Laundry&page="+pageno+"&hitsPerPage=25&facets=*&maxValuesPerFacet=5&facetFilters=%5B%5B%22state%3A"+locationstr+"%22%5D%2C%5B%5D%5D&aroundLatLngViaIP=false&aroundLatLng=&attributesToRetrieve=%5B%22businessid%22%2C%22description%22%2C%22businessurl%22%2C%22companylogo200%22%2C%22avgrating%22%2C%22topcategory%22%2C%22reviewcount%22%2C%22Likes%22%2C%22status%22%2C%22state%22%2C%22city%22%2C%22area%22%2C%22businessname%22%2C%22InsuredBadge%22%2C%22Mastercard%22%2C%22NewBadge%22%2C%22XpertBadge%22%2C%22TrustBadge%22%2C%22YearOnBadge%22%2C%22visitcount%22%2C%22hired%22%2C%22respond%22%2C%22lastactivedate%22%5D\"}";

                    //responseString = Encoding.UTF8.GetString(client.UploadValues(@site_api, postData));
                    responseString = client.UploadString(site_api,pData);
                    Newtonsoft.Json.Linq.JObject jObject = Newtonsoft.Json.Linq.JObject.Parse(responseString);

                    while (jObject["hits"].Count()!=0)
                    {
                        for (int i = 0; i < jObject["hits"].Count(); i++)
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { progressbar.Value = 25; });
                            //grab the url
                            string serviceurl = (string)jObject["hits"][i]["businessurl"];
                            string servicemainpage = client.DownloadString(site_url + serviceurl);
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { progressbar.Value = 50; });
                            Match email = Regex.Match(servicemainpage, "\"email\":\"(.*?)\"");
                            Match name = Regex.Match(servicemainpage, "\"businessname\":\"(.*?)\"");
                            Match mobile = Regex.Match(servicemainpage, "\"phone\":\"(.*?)\"");
                            Match website = Regex.Match(servicemainpage, "\"website\":\"(.*?)\"");
                            Match contactperson = Regex.Match(servicemainpage, "\"contactperson\":\"(.*?)\"");
                            Match alternatephone = Regex.Match(servicemainpage, "\"alternatephone\":\"(.*?)\"");
                            Match location = Regex.Match(servicemainpage, "\"state\":\"(.*?)\"");
                            Match address = Regex.Match(servicemainpage, "\"fulladdress\":\"(.*?)\"");
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { progressbar.Value = 70; });
                            
                            LaundryCompany lc = new LaundryCompany();
                            lc.Name = name.Groups[1].Value;
                            lc.Email = email.Groups[1].Value;
                            lc.Mobile = mobile.Groups[1].Value;
                            lc.Website = website.Groups[1].Value;
                            lc.ContactPerson = contactperson.Groups[1].Value;
                            lc.AlternatePhone = alternatephone.Groups[1].Value;
                            lc.Location = location.Groups[1].Value;
                            lc.Address = address.Groups[1].Value;


                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                            {
                                myEmails.Add(lc);
                                progressbar.Value = 100;
                                totalProgress = +5;
                                total_progressbar.Value = totalProgress;
                                //MessageBox.Show(email.Groups[1].Value);
                                // vconnectDG.ItemsSource = myEmails;
                            });
                        }
                        pageno=pageno+1;
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                        {
                            pagetxt.Text = pageno.ToString();
                        });
                        pData = "{\"params\":\"query=Laundry&page=" + pageno + "&hitsPerPage=25&facets=*&maxValuesPerFacet=5&facetFilters=%5B%5B%22state%3A" + locationstr + "%22%5D%2C%5B%5D%5D&aroundLatLngViaIP=false&aroundLatLng=&attributesToRetrieve=%5B%22businessid%22%2C%22description%22%2C%22businessurl%22%2C%22companylogo200%22%2C%22avgrating%22%2C%22topcategory%22%2C%22reviewcount%22%2C%22Likes%22%2C%22status%22%2C%22state%22%2C%22city%22%2C%22area%22%2C%22businessname%22%2C%22InsuredBadge%22%2C%22Mastercard%22%2C%22NewBadge%22%2C%22XpertBadge%22%2C%22TrustBadge%22%2C%22YearOnBadge%22%2C%22visitcount%22%2C%22hired%22%2C%22respond%22%2C%22lastactivedate%22%5D\"}";

                        //responseString = Encoding.UTF8.GetString(client.UploadValues(@site_api, postData));
                        responseString = client.UploadString(site_api, pData);
                        jObject = Newtonsoft.Json.Linq.JObject.Parse(responseString);
                    }

                        
                }
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    progressbar.IsEnabled = false;//progress starts simulating work done
                    progressbar.Visibility = Visibility.Collapsed;//progressbar visible
                    searchbtn.IsEnabled = true;//disable the search button until the work is done
                    locationtxt.IsEnabled = true;
                    pagetxt.IsEnabled = true;
                    total_progressbar.Visibility = Visibility.Collapsed;
                    //MessageBox.Show(email.Groups[1].Value);
                    // vconnectDG.ItemsSource = myEmails;
                });
            }
            catch (Exception ex)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    progressbar.IsEnabled = false;//progress starts simulating work done
                    progressbar.Visibility = Visibility.Collapsed;//progressbar visible
                    searchbtn.IsEnabled = true;//disable the search button until the work is done
                    locationtxt.IsEnabled = true;
                    pagetxt.IsEnabled = true;
                    total_progressbar.Visibility = Visibility.Collapsed;
                    MessageBox.Show(ex.Message.ToString());
                });
               
            }
                
        }

        
        /**

        private void generateEmail(String url)
        {
            string responseString;
            try
            {
                using (var client = new WebClient())
                {

                    responseString = client.DownloadString(@url);//fetch html page
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { progressbar.Value = 25; });//update progress bar
                    //match and get the bid codes for the request of their respective email
                    MatchCollection m1 = Regex.Matches(responseString, "onclick=\\\"lemail\\((.*?)\\)\\\"", RegexOptions.Singleline);

                    if (isUrlChanged == false)
                    {
                        MatchCollection m4 = Regex.Matches(responseString, "<a href=\\\"(.*?)\\\" class=\\\"astyle13_2\\\">", RegexOptions.Singleline);
                        changedUrl = m4[1].Groups[1].Value;
                        changedUrl = "http://www.listedin.biz" + changedUrl;
                        isUrlChanged = true;

                    }

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { progressbar.Value = 40; });

                    foreach (Match m in m1)//loop through to process and get the email addresses
                    {
                        string bdcode = m.Groups[1].Value;//get each bid code
                        Random rd = new Random();//needed as a prerequisite
                        DateTime dt = new DateTime();//same here
                        //same as this
                        double rndKey = rd.Next() * dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        using (var client2 = new WebClient())//send request for the email address
                        {
                            var values = new NameValueCollection();
                            values["bid"] = bdcode;//given the bid code
                            values["ps"] = "/directory/search-results.asp";
                            values["st"] = "1";
                            values["RandomKey"] = rndKey.ToString();//and random key generated
                            //save response which is a html page
                            var response = client2.UploadValues("http://www.listedin.biz/directory/ajax/process/other/eaddress.asp", values);
                            var responseString2 = Encoding.Default.GetString(response);
                            //match to get the email address from the html page
                            string bidCode;
                            MatchCollection m3 = Regex.Matches(responseString2, "value=\\\"(.*?)\\\"", RegexOptions.Singleline);
                            foreach (Match mt in m3)
                            {
                                bidCode = mt.Groups[1].Value;
                                bidnos.Add(bidCode);
                            }
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { progressbar.Value += 2.5; });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { progressbar.Value = 90; });
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                progressbar.Value = 100;
                for (int i = 0; i < bidnos.Count; i++)
                {
                    myEmails.Add(bidnos[i]);//save to the observable collection
                }
                celistview.ItemsSource = myEmails;//bind to the listview
                progressbar.Value = 0;
            });
        }


        **/

        private void browsefile_btn_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog initialization
            dlg = new OpenFileDialog();
            dlg.Filter = "Office Files|*.csv";
            dlg.Title = "csv files only";
            dlg.ShowDialog();
            //Start the appending to file chosen from the file dialog
            filename = dlg.FileName;
            filenameappend_txt.Text = filename;
            browsefile_btn.IsEnabled = false;
            Thread th = new Thread(appendtofile);//Thread
            th.Start();//Started
        }

        private void appendtofile()
        {
            FileStream f;
            try
            {
                f = new FileStream(filename, FileMode.Open);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { browsefile_btn.IsEnabled = true; });
                return;
            }
            //Reader to know current status of the file,that is length
            StreamReader sr = new StreamReader(f);
            int count = 0;
            while (sr.ReadLine() != null)
            {
                count += 1;
            }
            //Write to the file
            using (StreamWriter wr = new StreamWriter(f))
            {
               /** foreach (string m in myEmails)
                {
                    wr.WriteLine(count + "  ,  " + m);
                    count += 1;
                }**/
            }

            f.Close();
            sr.Close();

            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate()
            {
                browsefile_btn.IsEnabled = true;
                MessageBox.Show("File Updated\n The file has been appended as requested");
            });
        }

        private void savefile_btn_Click(object sender, RoutedEventArgs e)
        {
            th.Abort();
            progressbar.IsEnabled = false;//progress starts simulating work done
            progressbar.Visibility = Visibility.Collapsed;//progressbar visible
            searchbtn.IsEnabled = true;//disable the search button until the work is done
            locationtxt.IsEnabled = true;
            pagetxt.IsEnabled = true;
            total_progressbar.Visibility = Visibility.Collapsed;

            int count = 1;//numbers the email addresses
            //naming of file
            Random rd = new Random();
            string nwfilename = "vconnectbubbuecontact" + rd.Next(9000) + ".csv";
            string newfile = @"..\..\..\..\..\..\..\Documents\" + nwfilename;
            FileStream f = new FileStream(newfile, FileMode.Create);
            //writing to file
            using (StreamWriter sw = new StreamWriter(f))
            {
                sw.WriteLine("No , Name, Mobile, Email, Website, ContactPerson, AlternatePhone, Location, Address");
                for (int i = 0; i < myEmails.Count;i++ )
                {
                    sw.WriteLine(count + "  ,  " + myEmails[i].Name + "  ,  " + myEmails[i].Mobile + "  ,  " + myEmails[i].Email + "  ,  " + myEmails[i].Website + "  ,  " + myEmails[i].ContactPerson + "  ,  " + myEmails[i].AlternatePhone + "  ,  " + myEmails[i].Location + "  ,  " + myEmails[i].Address.Replace(","," "));
                    count++;
                }
            }
            //Displaying file info
            FileInfo f_info = new FileInfo(newfile);
            this.ShowMessageAsync("Saved File Info", "The file name is: " + nwfilename + "\n File path is:" + f_info.DirectoryName);
            f.Close();
            savefile_btn.IsEnabled = false;

        }

        private void searchtxt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            locationtxt.Text = "";//Clears text box
        }

        /**
        private void auto_append_chkbx_Checked(object sender, RoutedEventArgs e)
        {
            if (auto_append_chkbx.IsChecked == true)
            {
                //from_value_txtbx.IsEnabled = true;
                //to_value_txtbx.IsEnabled = true;
                //from_value_txtbx.Text = "2";
                //to_value_txtbx.Text = "10";
            }
            else
            {
                //from_value_txtbx.Clear();
                //to_value_txtbx.Clear();
                //from_value_txtbx.IsEnabled = false;
                //to_value_txtbx.IsEnabled = false;
            }
        }
         **/
    }
}
