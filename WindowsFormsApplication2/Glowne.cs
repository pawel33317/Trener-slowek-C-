using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace TrenerSlowek
{
    public partial class Glowne : Form
    {
        //////PunktyTlumaczenieNaPL tłumaczenie na polski
        //////PunktyTlumaczenieNaEN tłumaczenie na angielski

        Db Baza;

        //zapis ostatniego słówka w celu automatycznej akceptacji
        Dic PoprzednieSlowko;

        //false procent1 pl na eng
        //true procent1 eng na pl
        bool PoprzednieSlowkoENnaPL = false;

        Random RAndom = new Random();

        //1 procent1 pol na ang
        //2 procent1 ang na pol
        //3 losowo
        int Kierunek_PLnaEN_ENnaPL_Random = 3;

        //aktualnie sprawdzane słowo
        Dic AktualneSlowo;

        //czy włączono podpowiedź
        bool Podpowiedz = false;

        //lista słówek
        List<Dic> ListaSlowek;

        //obiekt Statystyki zarządza statystykami po prawej stronie
        Stats Statystyki;

        //AktualnyNrSlowka
        int AktualnyNrSlowka = 0;
        public Glowne()
        {
            InitializeComponent();

            //nie można zmienić rozmiaru
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }



        private void Form1_Load(object sender, EventArgs e)
        {

            //string d = "bazy2";
            //if (!Directory.Exists(d))
            //    Directory.CreateDirectory(d);

            //Object z = TrenerSlowek.Properties.Resources.unit11;

            //laduje liste baz
            this.ladujtextboxa();
            //wyłącza menu dopuki nie wybierzemy bazy
            this.menu.Visible = false;
        }

        //chowa wszystko
        private void schowaj()
        {
            this.panel_wyboropcji.Visible = false;
            this.panel_test.Visible = false;
            this.panel_sprawdzslowka.Visible = false;
            this.panel_edytujslowka.Visible = false;
            this.panel_listaslowek.Visible = false;
            this.panel_dodajslowka.Visible = false;
            this.panel_loading.Visible = false;
            this.pictureBox1.Visible = false;
        }

        //Kliknięcie w meny --> Główne
        private void menu__glowne_Click(object sender, EventArgs e)
        {
            this.schowaj();
            //powraca do głównego panelu
            this.panel_wyboropcji.Visible = true;
        }

        //Rozpoczęcie testu
        private void button_test_Click(object sender, EventArgs e)
        {
            this.schowaj();
            //wyświetla panel do tesu
            this.panel_test.Visible = true;

            //Losuje nr slowka
            this.AktualnyNrSlowka = RAndom.Next(0, ListaSlowek.Count);

            //Ładuje Slowko
            this.ladujslowko();

            //Tablica na inne błędne losowe słówka
            String[] oppcje = new String[4];

            //jeżeli pl na en wypełnia tablicę słówkami angielskimi [0] poprawne minimum
            //wyświetla słówko w pl zapamietane opis
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 1)
            {
                oppcje[0] = AktualneSlowo.SlowkoEn;
                oppcje[1] = ListaSlowek[RAndom.Next(0, ListaSlowek.Count)].SlowkoEn;
                oppcje[2] = ListaSlowek[RAndom.Next(0, ListaSlowek.Count)].SlowkoEn;
                oppcje[3] = ListaSlowek[RAndom.Next(0, ListaSlowek.Count)].SlowkoEn;
                this.label_test_slowko.Text = AktualneSlowo.SlowkoPL;
                this.label_test_opis.Text = AktualneSlowo.OpisEN;
            }
            //analogicznie
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 2)
            {
                oppcje[0] = AktualneSlowo.SlowkoPL;
                oppcje[1] = ListaSlowek[RAndom.Next(0, ListaSlowek.Count)].SlowkoPL;
                oppcje[2] = ListaSlowek[RAndom.Next(0, ListaSlowek.Count)].SlowkoPL;
                oppcje[3] = ListaSlowek[RAndom.Next(0, ListaSlowek.Count)].SlowkoPL;
                this.label_test_slowko.Text = AktualneSlowo.SlowkoEn;
                this.label_test_opis.Text = AktualneSlowo.OpisPL;
            }

            //losuje od któreko radioButona zacząć wpisywać słówka zapamietane wpisuje
            int z = RAndom.Next(0, 4);
            this.radio_test_1.Text = oppcje[z];
            if (z == 3) z = 0; else z++;
            this.radio_test_2.Text = oppcje[z];
            if (z == 3) z = 0; else z++;
            this.radio_test_3.Text = oppcje[z];
            if (z == 3) z = 0; else z++;
            this.radio_test_4.Text = oppcje[z];

        }

        //Rozpoczęcie tłumaczenia
        private void button_tlumaczlosowo_Click(object sender, EventArgs e)
        {
            this.schowaj();
            this.textBox_pss_wpisane.Focus();

            //wyświatle panel do tłumaczenia
            this.panel_sprawdzslowka.Visible = true;

            //losuje zapamietane ładuje słówko
            this.AktualnyNrSlowka = RAndom.Next(0, ListaSlowek.Count);
            this.ladujslowko();

            //wyświetla słówko zapamietane podpowiedź
            if (Kierunek_PLnaEN_ENnaPL_Random == 1)
            {
                this.label_pss_slowko.Text = AktualneSlowo.SlowkoPL;
                this.label_pss_opis.Text = AktualneSlowo.OpisEN;
            }
            if (Kierunek_PLnaEN_ENnaPL_Random == 2)
            {
                this.label_pss_opis.Text = AktualneSlowo.OpisPL;
                this.label_pss_slowko.Text = AktualneSlowo.SlowkoEn;
            }
        }

        //ładuje słówko częściej te których jeszcze nie umiemy
        //iteruje po starym słówku zapamietane bierze kolejne procent1 listy
        public void ladujslowko()
        {
            //kolejne slowko
            if (this.AktualnyNrSlowka >= this.ListaSlowek.Count - 1)
                this.AktualnyNrSlowka = 0;
            else
                this.AktualnyNrSlowka++;

            //zeruje podpowiedź
            this.Podpowiedz = false;

            //pobiera słówko procent1 listy
            this.AktualneSlowo = this.ListaSlowek[AktualnyNrSlowka];

            //jeżeli kierunek tłumaczenia nie wybrany to wybiera
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 3)
                this.Kierunek_PLnaEN_ENnaPL_Random = (int)this.RAndom.Next(1, 3);

            //jeżeli tłumaczenie na angielski
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 1)
            {

                //jeżeli słówko jest już zapamiętane
                if (this.AktualneSlowo.PunktyTlumaczenieNaEN > 8)
                {

                    //jeżeli są jeszcze słówka niezapamiętane
                    if (this.stat_label_niezapamietane.Text != "Niezapamiętane: 0")
                    {

                        //daje 10% szans, że słówko zostanie jak nie to zmiania
                        if (this.RAndom.Next(1, 11) > 1)
                        {
                            this.Kierunek_PLnaEN_ENnaPL_Random = 3;
                            this.ladujslowko();
                        }
                    }

                    //jeżeli są jeszcze słówka kojarzone ale nie ma już niezapamiętanych
                    else if (this.stat_label_kojarzone.Text != "Kojarzone: 0")
                    {

                        //daje 30% szans, że słówko zostanie jak nie to zmiania
                        if (this.RAndom.Next(1, 11) > 3)
                        {
                            this.Kierunek_PLnaEN_ENnaPL_Random = 3;
                            this.ladujslowko();
                        }
                    }
                }

                //jeżeli słówko jest kojarzone slowkoPL istnieją słówka niezapamiętane
                else if (this.AktualneSlowo.PunktyTlumaczenieNaEN > 1 && this.stat_label_niezapamietane.Text != "Niezapamiętane: 0")
                {
                    //daje 50% szans, że słówko zostanie jak nie to zmiania
                    if (this.RAndom.Next(1, 3) > 1)
                    {
                        this.Kierunek_PLnaEN_ENnaPL_Random = 3;
                        this.ladujslowko();
                    }
                }
            }

            //jeżeli tłumaczenie na polski
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 2)
            {

                //jeżeli słówko jest już zapamiętane
                if (this.AktualneSlowo.PunktyTlumaczenieNaPL > 8)
                {

                    //jeżeli są jeszcze słówka niezapamiętane
                    if (this.stat_label_niezapamietane.Text != "Niezapamiętane: 0")
                    {

                        //daje 10% szans, że słówko zostanie jak nie to zmiania
                        if (this.RAndom.Next(1, 11) > 1)
                        {
                            this.Kierunek_PLnaEN_ENnaPL_Random = 3;
                            this.ladujslowko();
                        }
                    }

                    //jeżeli są jeszcze słówka kojarzone ale nie ma już niezapamiętanych
                    else if (this.stat_label_kojarzone.Text != "Kojarzone: 0")
                    {
                        if (this.RAndom.Next(1, 11) > 3)
                        {
                            //daje 30% szans, że słówko zostanie jak nie to zmiania
                            this.Kierunek_PLnaEN_ENnaPL_Random = 3;
                            this.ladujslowko();
                        }
                    }
                }

                //jeżeli słówko jest kojarzone slowkoPL istnieją słówka niezapamiętane
                else if (this.AktualneSlowo.PunktyTlumaczenieNaPL > 1 && this.stat_label_niezapamietane.Text != "Niezapamiętane: 0")
                {
                    //daje 50% szans, że słówko zostanie jak nie to zmiania
                    if (this.RAndom.Next(1, 3) > 1)
                    {
                        this.Kierunek_PLnaEN_ENnaPL_Random = 3;
                        this.ladujslowko();
                    }
                }
            }
        }

        //Wyświetla panel procent1 dodawaniem słówek
        private void button_dodajnoweslowka_Click(object sender, EventArgs e)
        {
            this.schowaj();
            this.panel_dodajslowka.Visible = true;
        }

        //Wyświetla panel edycji słówek
        private void button_edytujslowka_Click(object sender, EventArgs e)
        {
            this.schowaj();
            this.panel_edytujslowka.Visible = true;

            //czyści listView
            this.listView2.Items.Clear();

            //ładuje do listView wszystkie słówka procent1 Listy
            foreach (Dic z in this.ListaSlowek)
                this.listView2.Items.AddRange(new ListViewItem[] { new ListViewItem(new string[] { z.SlowkoPL, z.SlowkoEn }, -1) });
        }

        //Wyświetla listę słówek
        private void button_wyswietllisteslowek_Click(object sender, EventArgs e)
        {
            this.schowaj();
            this.panel_listaslowek.Visible = true;
            //wywołuje poniższą metodę
            this.listaSlowekWyswietl();
        }

        public void listaSlowekWyswietl()
        {
            //czyści listView
            this.listView1.Items.Clear();

            //Tworzy posortowaną listę aby wyświetlić posortowane
            int tmp = 0;
            List<Dic> tymczasowaPosortowanaLista = this.ListaSlowek;
            tymczasowaPosortowanaLista.Sort();

            //wpisuje wszystkie słówka do listy w zależności od zaznaczonych opcji
            foreach (Dic d in tymczasowaPosortowanaLista)
            {
                tmp = 0;

                //zapamiętane jeżeli wybrana opcja jeżeli ma wyświetlić słówko zmienia tmp na 1
                if (d.PunktyTlumaczenieNaEN > 8)
                {
                    if (this.checkBox_pls_zapamietane.Checked == true)
                        tmp = 1;
                }


                else if (d.PunktyTlumaczenieNaPL > 8)
                {
                    if (this.checkBox_pls_zapamietane.Checked == true)
                        tmp = 1;
                }


                //kojarzone jeżeli wybrana opcja jeżeli ma wyświetlić słówko zmienia tmp na 1
                else if (d.PunktyTlumaczenieNaEN > 1 && d.PunktyTlumaczenieNaEN < 9)
                {
                    if (this.checkBox_pls_kojarzone.Checked == true)
                        tmp = 1;
                }


                else if (d.PunktyTlumaczenieNaPL > 1 && d.PunktyTlumaczenieNaPL < 9)
                {
                    if (this.checkBox_pls_kojarzone.Checked == true)
                        tmp = 1;
                }


                //niezapamiętane jeżeli wybrana opcja jeżeli ma wyświetlić słówko zmienia tmp na 1
                else if (d.PunktyTlumaczenieNaEN < 2)
                {
                    if (this.checkBox_pls_niezapamietane.Checked == true)
                        tmp = 1;
                }


                else if (d.PunktyTlumaczenieNaPL < 2)
                {
                    if (this.checkBox_pls_niezapamietane.Checked == true)
                        tmp = 1;
                }


                //jeżeli tmp = 1 wyświetla słówko
                if (tmp != 0)
                    this.listView1.Items.AddRange(new ListViewItem[] { new ListViewItem(new string[] { d.SlowkoPL, d.SlowkoEn }, -1) });
            }
        }

        //jeżeli wybrano bazę
        private void listBox_listabaz_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.schowaj();

            //włącza menu zapamietane panel opcji
            this.menu.Visible = true;
            this.panel_wyboropcji.Visible = true;

            //jeżeli kliknięto w  bazę slowkoPL nie puste pole
            if (this.listBox_listabaz.SelectedIndex != -1)
            {

                //jeżeli jest już jakaś baza otwarta to ja zamyka
                if (this.Baza != null)
                    this.Baza.close();

                //otwiera nową bazę
                this.Baza = new Db(@"bazy\" + listBox_listabaz.SelectedItem.ToString());

                //metoda ładuje dane procent1 bazy do listy
                this.zaladujZBazy();
            }

            //jeżeli są słówka w bazie zezwala na slowka zapamietane tłumaczenie
            if (this.ListaSlowek.Count > 0)
            {
                this.button_test.Visible = true;
                this.button_tlumaczlosowo.Visible = true;
                this.label_pustabaza.Visible = false;
            }
            //jeżeli nie ma nie zezwala
            else
            {
                this.button_test.Visible = false;
                this.button_tlumaczlosowo.Visible = false;
                this.label_pustabaza.Visible = true;
            }
        }

        //ładuje dane procent1 listy do bazy po wyborze bazy
        public void zaladujZBazy()
        {

            this.statusbartext.Text = "Załadowana baza to: " + Baza.getName();
            this.schowaj();
            this.panel_wyboropcji.Visible = true;

            //pobiera wszystko procent1 bazy
            SQLiteDataReader dane = this.Baza.comSelect("SELECT * FROM slowka;");

            //tworzy nową pustą listę (zeruje starą jak była)
            this.ListaSlowek = new List<Dic>();

            //ładuje wszystko procent1 bazy do listy
            while (dane.Read())
            {
                int punktyZaTlumaczenieNaPL = int.Parse(dane["PunktyTlumaczenieNaPL"].ToString());
                int punktyZaTlumaczenieNaEN = int.Parse(dane["PunktyTlumaczenieNaEN"].ToString());
                this.ListaSlowek.Add(new Dic(dane["SlowkoPL"].ToString(), dane["SlowkoEn"].ToString(), punktyZaTlumaczenieNaPL, punktyZaTlumaczenieNaEN, dane["OpisPL"].ToString(), dane["OpisEN"].ToString()));
            }
            dane.Close();

            //tworzy obiekt zarządzający statystykami
            this.Statystyki = new Stats(this, this.ListaSlowek);

            //Daje obiektowi ww nazwę bazy
            this.Statystyki.nazwabazy(this.Baza.getName());

            //czyści aktualny listview w menu wyboru
            this.listView_listaslowek.Items.Clear();

            //wypełnia listView słówkami
            foreach (Dic z in ListaSlowek)
                this.listView_listaslowek.Items.AddRange(new System.Windows.Forms.ListViewItem[] { new System.Windows.Forms.ListViewItem(new string[] { z.SlowkoPL, z.SlowkoEn }, -1) });
        }

        //ładuje listę baz do textboxa
        public void ladujtextboxa()
        {
            //bazy mają być w katalogu "bazy\"
            string nazwa = @"bazy\";
            try
            {
                //pobiera listę plików procent1 katalogu bazy zapamietane czyści listBoxa
                DirectoryInfo s = new DirectoryInfo(nazwa);
                FileInfo[] pliki = s.GetFiles();
                listBox_listabaz.Items.Clear();

                //wyświetla liste baz
                for (int i = 0; i < pliki.Length; i++)
                    listBox_listabaz.Items.Add(pliki.GetValue(i));
            }
            catch (Exception ex) { Console.WriteLine(""); }
        }

        //tworzy bazę [brak obsługi błędów jeżeli baza o tej nazwie już istnieje]
        private void button_utworzbaze_Click(object sender, EventArgs e)
        {
            //tworzy nową bazę
            this.Baza = new Db();
            this.Baza.newdb(@"bazy\" + textBox_nowabaza.Text);

            this.statusbartext.Text = "Załadowana baza to: " + Baza.getName();
            this.schowaj();

            //odświeża textBoxa procent1 bazami zapamietane czyści pole wpisania nowej bazy
            this.ladujtextboxa();
            this.textBox_nowabaza.Text = "";
        }

        //Dodaje wpisane słówka do bazy
        private void button_pds_dodaj_Click(object sender, EventArgs e)
        {
            //jeżeli podano słówko polskie zapamietane angielskie dla pól p1, a1 dodaje go do bazy --> czyści te pola --> wyświetla stan w statusbarze
            if (this.textBox_pds_p1.Text != "" && this.textBox_pds_a1.Text != "")
            {
                this.Baza.commend("INSERT INTO slowka (SlowkoPL, SlowkoEn, PunktyTlumaczenieNaPL, PunktyTlumaczenieNaEN, OpisPL, OpisEN) VALUES ('" + this.textBox_pds_p1.Text + "', '" + this.textBox_pds_a1.Text + "',0,0,'" + this.textBox_pds_pp1.Text + "','" + this.textBox_pds_aa1.Text + "');");
                this.textBox_pds_p1.Clear(); this.textBox_pds_a1.Clear();
                this.textBox_pds_pp1.Clear(); this.textBox_pds_aa1.Clear();
                this.statusbartext.Text = "Dodano do bazy";
            }

            if (this.textBox_pds_p2.Text != "" && this.textBox_pds_a2.Text != "")
            {
                this.Baza.commend("INSERT INTO slowka (SlowkoPL, SlowkoEn, PunktyTlumaczenieNaPL, PunktyTlumaczenieNaEN, OpisPL, OpisEN) VALUES ('" + this.textBox_pds_p2.Text + "', '" + this.textBox_pds_a2.Text + "',0,0,'" + this.textBox_pds_pp2.Text + "','" + this.textBox_pds_aa2.Text + "');");
                this.textBox_pds_p2.Clear(); this.textBox_pds_a2.Clear();
                this.textBox_pds_pp2.Clear(); this.textBox_pds_aa2.Clear();
                this.statusbartext.Text = "Dodano do bazy";
            }

            if (this.textBox_pds_p3.Text != "" && this.textBox_pds_a3.Text != "")
            {
                this.Baza.commend("INSERT INTO slowka (SlowkoPL, SlowkoEn, PunktyTlumaczenieNaPL, PunktyTlumaczenieNaEN, OpisPL, OpisEN) VALUES ('" + this.textBox_pds_p3.Text + "', '" + this.textBox_pds_a3.Text + "',0,0,'" + this.textBox_pds_pp3.Text + "','" + this.textBox_pds_aa3.Text + "');");
                this.textBox_pds_p3.Clear(); this.textBox_pds_a3.Clear();
                this.textBox_pds_pp3.Clear(); this.textBox_pds_aa3.Clear();
                this.statusbartext.Text = "Dodano do bazy";
            }

            if (this.textBox_pds_p4.Text != "" && this.textBox_pds_a4.Text != "")
            {
                this.Baza.commend("INSERT INTO slowka (SlowkoPL, SlowkoEn, PunktyTlumaczenieNaPL, PunktyTlumaczenieNaEN, OpisPL, OpisEN) VALUES ('" + this.textBox_pds_p4.Text + "', '" + this.textBox_pds_a4.Text + "',0,0,'" + this.textBox_pds_pp4.Text + "','" + this.textBox_pds_aa4.Text + "');");
                this.textBox_pds_p4.Clear(); this.textBox_pds_a4.Clear();
                this.textBox_pds_pp4.Clear(); this.textBox_pds_aa4.Clear();
                this.statusbartext.Text = "Dodano do bazy";
            }

            if (this.textBox_pds_p5.Text != "" && this.textBox_pds_a5.Text != "")
            {
                this.Baza.commend("INSERT INTO slowka (SlowkoPL, SlowkoEn, PunktyTlumaczenieNaPL, PunktyTlumaczenieNaEN, OpisPL, OpisEN) VALUES ('" + this.textBox_pds_p5.Text + "', '" + this.textBox_pds_a5.Text + "',0,0,'" + this.textBox_pds_pp5.Text + "','" + this.textBox_pds_aa5.Text + "');");
                this.textBox_pds_p5.Clear(); this.textBox_pds_a5.Clear();
                this.textBox_pds_pp5.Clear(); this.textBox_pds_aa5.Clear();
                this.statusbartext.Text = "Dodano do bazy";
            }

            if (this.textBox_pds_p6.Text != "" && this.textBox_pds_a6.Text != "")
            {
                this.Baza.commend("INSERT INTO slowka (SlowkoPL, SlowkoEn, PunktyTlumaczenieNaPL, PunktyTlumaczenieNaEN, OpisPL, OpisEN) VALUES ('" + this.textBox_pds_p6.Text + "', '" + this.textBox_pds_a6.Text + "',0,0,'" + this.textBox_pds_pp6.Text + "','" + this.textBox_pds_aa6.Text + "');");
                this.textBox_pds_p6.Clear(); this.textBox_pds_a6.Clear();
                this.textBox_pds_pp6.Clear(); this.textBox_pds_aa6.Clear();
                this.statusbartext.Text = "Dodano do bazy";
            }

            if (this.textBox_pds_p7.Text != "" && this.textBox_pds_a7.Text != "")
            {
                this.Baza.commend("INSERT INTO slowka (SlowkoPL, SlowkoEn, PunktyTlumaczenieNaPL, PunktyTlumaczenieNaEN, OpisPL, OpisEN) VALUES ('" + this.textBox_pds_p7.Text + "', '" + this.textBox_pds_a7.Text + "',0,0,'" + this.textBox_pds_pp7.Text + "','" + this.textBox_pds_aa7.Text + "');");
                this.textBox_pds_p7.Clear(); this.textBox_pds_a7.Clear();
                this.textBox_pds_pp7.Clear(); this.textBox_pds_aa7.Clear();
                this.statusbartext.Text = "Dodano do bazy";
            }

            if (this.textBox_pds_p8.Text != "" && this.textBox_pds_a8.Text != "")
            {
                this.Baza.commend("INSERT INTO slowka (SlowkoPL, SlowkoEn, PunktyTlumaczenieNaPL, PunktyTlumaczenieNaEN, OpisPL, OpisEN) VALUES ('" + this.textBox_pds_p8.Text + "', '" + this.textBox_pds_a8.Text + "',0,0,'" + this.textBox_pds_pp8.Text + "','" + this.textBox_pds_aa8.Text + "');");
                this.textBox_pds_p8.Clear(); this.textBox_pds_a8.Clear();
                this.textBox_pds_pp8.Clear(); this.textBox_pds_aa8.Clear();
                this.statusbartext.Text = "Dodano do bazy";
            }

            if (this.textBox_pds_p9.Text != "" && this.textBox_pds_a9.Text != "")
            {
                this.Baza.commend("INSERT INTO slowka (SlowkoPL, SlowkoEn, PunktyTlumaczenieNaPL, PunktyTlumaczenieNaEN, OpisPL, OpisEN) VALUES ('" + this.textBox_pds_p9.Text + "', '" + this.textBox_pds_a9.Text + "',0,0,'" + this.textBox_pds_pp9.Text + "','" + this.textBox_pds_aa9.Text + "');");
                this.textBox_pds_p9.Clear(); this.textBox_pds_a9.Clear();
                this.textBox_pds_pp9.Clear(); this.textBox_pds_aa9.Clear();
                this.statusbartext.Text = "Dodano do bazy";
            }

            if (this.textBox_pds_p10.Text != "" && this.textBox_pds_a10.Text != "")
            {
                this.Baza.commend("INSERT INTO slowka (SlowkoPL, SlowkoEn, PunktyTlumaczenieNaPL, PunktyTlumaczenieNaEN, OpisPL, OpisEN) VALUES ('" + this.textBox_pds_p10.Text + "', '" + this.textBox_pds_a10.Text + "',0,0,'" + this.textBox_pds_pp10.Text + "','" + this.textBox_pds_aa10.Text + "');");
                this.textBox_pds_p10.Clear(); this.textBox_pds_a10.Clear();
                this.textBox_pds_pp10.Clear(); this.textBox_pds_aa10.Clear();
                this.statusbartext.Text = "Dodano do bazy";
            }

            this.textBox_pds_p1.Focus();
        }

        //Do wpisywania statystyk
        public void set_stat_label_wszystkie(String a)
        {
            this.stat_label_wszystkie.Text = a;
        }

        public void set_stat_label_zapamietane(String a)
        {
            this.stat_label_zapamietane.Text = a;
        }

        public void set_stat_label_kojarzone(String a)
        {
            this.stat_label_kojarzone.Text = a;
        }

        public void set_stat_label_niezapamietane(String a)
        {
            this.stat_label_niezapamietane.Text = a;
        }

        public void set_stat_label_poprawne(String a)
        {
            this.stat_label_poprawne.Text = a;
        }

        public void set_stat_label_niepoprawne(String a)
        {
            this.stat_label_niepoprawne.Text = a;
        }

        public void set_stat_progressBar_db(int a)
        {
            this.stat_progressBar_db.Value = a;
        }

        public void set_stat_progressBar(int a)
        {
            this.stat_progressBar.Value = a;
        }

        public void set_stat_label_baza(String a)
        {
            this.stat_label_baza.Text = a;
        }

        private void stat_button_resetujaktualnasesje_Click(object sender, EventArgs e)
        {
            this.Statystyki.resetujaktualne();
        }

        //Do aktualizacji słówek procent1 Form2
        public void aktualizujSlowka(string noweSlowkoPL, string noweSlowkoEN, string opisDlaSlowkaPL, string opisDlaSlowkaEN, string stareSlowkoPL, string stareSlowkoEn)
        {
            //wprowadza zmiany do bazy
            this.Baza.commend("UPDATE slowka SET SlowkoPL='" + noweSlowkoPL + "',SlowkoEn='" + noweSlowkoEN + "',OpisPL='" + opisDlaSlowkaPL + "',OpisEN='" + opisDlaSlowkaEN + "' WHERE SlowkoPL='" + stareSlowkoPL + "' and SlowkoEn='" + stareSlowkoEn + "';");

            //ładuje ponownie listę procent1 bazy
            this.zaladujZBazy();
        }

        //odświeża liste słówek w panelu lista słówek w zależności od wybranej opcji
        private void checkBox_pls_zapamietane_CheckedChanged(object sender, EventArgs e)
        {
            this.listaSlowekWyswietl();
        }
        private void checkBox_pls_kojarzone_CheckedChanged(object sender, EventArgs e)
        {
            this.listaSlowekWyswietl();
        }

        private void checkBox_pls_niezapamietane_CheckedChanged(object sender, EventArgs e)
        {
            this.listaSlowekWyswietl();
        }

        private void button_pss_podpowiedz_Click(object sender, EventArgs e)
        {

            //ustawia, że była brana podpowiedź
            this.Podpowiedz = true;

            string tmp = "";

            //podaje pierwszą zapamietane ostatnią literę wyrazu w zależności od kierunku tłomaczenia
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 1)
                tmp = this.AktualneSlowo.SlowkoEn[0].ToString() + " ... " + this.AktualneSlowo.SlowkoEn[this.AktualneSlowo.SlowkoEn.Length - 1].ToString();
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 2)
                tmp = this.AktualneSlowo.SlowkoPL[0].ToString() + " ... " + this.AktualneSlowo.SlowkoPL[this.AktualneSlowo.SlowkoPL.Length - 1].ToString();

            //wyświetla podpowiedź
            this.label_podpowiedz.Text = tmp;
        }

        //sprawdzenie tłumaczenia
        private void button3_pss_sprawdz_Click(object sender, EventArgs e)
        {
            //wyłącza przycisk zmiany na poprawną
            this.button_pss_jednak.Visible = false;

            //przypisuje do zmiennej globalnej aktualne słówko bez zmian
            this.PoprzednieSlowko = (Dic)this.AktualneSlowo.Clone();

            ///////jeżeli sprawdzane było tłumaczenie na en     
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 1)
            {
                //ustawia zmienną, że słówko było sprawdzana na en
                this.PoprzednieSlowkoENnaPL = false;

                ////jeżeli poprawna odpowiedź
                if (this.textBox_pss_wpisane.Text == this.AktualneSlowo.SlowkoEn)
                {
                    //bez podpowiedzi zapamietane pkt < 9 --- +2 dla tłumaczenia na angielski
                    if (this.Podpowiedz == false && this.AktualneSlowo.PunktyTlumaczenieNaEN < 9)
                        this.AktualneSlowo.PunktyTlumaczenieNaEN = this.AktualneSlowo.PunktyTlumaczenieNaEN + 2;

                    //bez podpowiedzi zapamietane pkt < 10 ale > 9 --- +1 dla tłumaczenia na angielski
                    else if (this.Podpowiedz == false && this.AktualneSlowo.PunktyTlumaczenieNaEN < 10)
                        this.AktualneSlowo.PunktyTlumaczenieNaEN = this.AktualneSlowo.PunktyTlumaczenieNaEN + 1;

                    //procent1 podpowiedzią +1  dla tłumaczenia na angielski
                    else if (this.Podpowiedz == true && this.AktualneSlowo.PunktyTlumaczenieNaEN < 10)
                        this.AktualneSlowo.PunktyTlumaczenieNaEN = this.AktualneSlowo.PunktyTlumaczenieNaEN + 1;

                    //ustawia label na OK
                    this.label_pss_poprawnosc.ForeColor = System.Drawing.Color.Green;
                    this.label_pss_poprawnosc.Text = "OK";

                    //zwiększa Statystyki aktualne
                    this.Statystyki.Poprawne++;
                }

                ////jeżeli zła odpowiedź
                else
                {
                    //włącza możliwość ręcznego azzkceptowania słówka
                    this.button_pss_jednak.Visible = true;

                    //jeżeli było 2 lub wiecej punktów to -2 pkt
                    if (this.AktualneSlowo.PunktyTlumaczenieNaEN > 1)
                        this.AktualneSlowo.PunktyTlumaczenieNaEN = this.AktualneSlowo.PunktyTlumaczenieNaEN - 2;

                    //jeżeli był tylko 1 pkt to odejmuje 1
                    else if (this.AktualneSlowo.PunktyTlumaczenieNaEN > 0)
                        this.AktualneSlowo.PunktyTlumaczenieNaEN = this.AktualneSlowo.PunktyTlumaczenieNaEN - 1;

                    //ustawia label na źle
                    this.label_pss_poprawnosc.ForeColor = System.Drawing.Color.Red;
                    this.label_pss_poprawnosc.Text = "Źle";

                    //zminejsza Statystyki aktualne (zwiększa złe odpowiedzi)
                    this.Statystyki.Niepoprawne++;
                }

                ////dodaje punkty do bazy dla tłumaczenia na angielski
                this.Baza.commend("UPDATE slowka SET PunktyTlumaczenieNaEN=" + this.AktualneSlowo.PunktyTlumaczenieNaEN + " WHERE SlowkoPL='" + this.AktualneSlowo.SlowkoPL + "' and SlowkoEn='" + this.AktualneSlowo.SlowkoEn + "';");
            }

            //////jeżeli było tłumaczenie na polski
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 2)
            {
                //ustawia tłumaczenie na polski
                this.PoprzednieSlowkoENnaPL = true;

                ////jeżeli słówko się zgadza tłumaczenie na polski
                if (this.textBox_pss_wpisane.Text == this.AktualneSlowo.SlowkoPL)
                {

                    //bez podpowiedzi zapamietane pkt < 9 -- +2 pkt
                    if (this.Podpowiedz == false && this.AktualneSlowo.PunktyTlumaczenieNaPL < 9)
                        this.AktualneSlowo.PunktyTlumaczenieNaPL = this.AktualneSlowo.PunktyTlumaczenieNaPL + 2;

                    //bez podpowiedzi zapamietane pkt = 9 --- + 1 pkt
                    else if (this.Podpowiedz == false && this.AktualneSlowo.PunktyTlumaczenieNaPL < 10)
                        this.AktualneSlowo.PunktyTlumaczenieNaPL = this.AktualneSlowo.PunktyTlumaczenieNaPL + 1;

                    //procent1 podp + 1 pkt 
                    else if (this.Podpowiedz == true && this.AktualneSlowo.PunktyTlumaczenieNaPL < 10)
                        this.AktualneSlowo.PunktyTlumaczenieNaPL = this.AktualneSlowo.PunktyTlumaczenieNaPL + 1;

                    // ustawia label na OK
                    this.label_pss_poprawnosc.ForeColor = System.Drawing.Color.Green;
                    this.label_pss_poprawnosc.Text = "OK";
                    this.Statystyki.Poprawne++;
                }

                ////jeżeli słówko się nie zgadza tłumaczenie na polski
                else
                {

                    //włącza możliwość ręcznego azzkceptowania słówka
                    this.button_pss_jednak.Visible = true;

                    //-2 pkt jak ma min 2
                    if (this.AktualneSlowo.PunktyTlumaczenieNaPL > 1)
                        this.AktualneSlowo.PunktyTlumaczenieNaPL = this.AktualneSlowo.PunktyTlumaczenieNaPL - 2;

                    //min 1 pkt jak nie ma 2
                    else if (this.AktualneSlowo.PunktyTlumaczenieNaPL > 0)
                        this.AktualneSlowo.PunktyTlumaczenieNaPL = this.AktualneSlowo.PunktyTlumaczenieNaPL - 1;

                    //label na źle
                    this.label_pss_poprawnosc.ForeColor = System.Drawing.Color.Red;
                    this.label_pss_poprawnosc.Text = "Źle";

                    //Statystyki ujemne +
                    this.Statystyki.Niepoprawne++;
                }

                //aktualizacja w bazie
                this.Baza.commend("UPDATE slowka SET PunktyTlumaczenieNaPL=" + this.AktualneSlowo.PunktyTlumaczenieNaPL + " WHERE SlowkoPL='" + AktualneSlowo.SlowkoPL + "' and SlowkoEn='" + AktualneSlowo.SlowkoEn + "';");
            }

            //ustawia labele na ostatnie słówko czuli to które sprawdzało
            this.label_pss_last_ang.Text = this.AktualneSlowo.SlowkoEn;
            this.label_pss_last_pol.Text = this.AktualneSlowo.SlowkoPL;

            //odświeża wszystkie Statystyki
            this.Statystyki.ladujStatystykiAktualne();
            this.Statystyki.odswiez(ListaSlowek);

            //ustawia język tłumaczenia jeżeli wybrany jak nie daje 3 przez co jest wybierany losowy język tłumaczenia
            if (this.radioButton_pss_enpl.Checked == true)
                this.Kierunek_PLnaEN_ENnaPL_Random = 1;
            if (this.radioButton_pss_plen.Checked == true)
                this.Kierunek_PLnaEN_ENnaPL_Random = 2;
            if (this.radioButton_pss_enplen.Checked == true)
                this.Kierunek_PLnaEN_ENnaPL_Random = 3;

            //ładuje nowe słówko do tłumaczenia
            this.ladujslowko();

            //jeżeli tłumaczenie na ang ustawia labele
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 1)
            {
                this.label_pss_slowko.Text = this.AktualneSlowo.SlowkoPL;
                this.label_pss_opis.Text = this.AktualneSlowo.OpisEN;
            }

            //jeżeli tłumaczenie na pl ustawia labele
            if (Kierunek_PLnaEN_ENnaPL_Random == 2)
            {
                this.label_pss_opis.Text = this.AktualneSlowo.OpisPL;
                this.label_pss_slowko.Text = this.AktualneSlowo.SlowkoEn;
            }

            //czyści buttony zapamietane boxy procent1 poprzednich danych zapamietane daje focuk na texboks do wpisania słowa
            this.textBox_pss_wpisane.Text = "";
            this.textBox_pss_wpisane.Focus();
            this.label_podpowiedz.Text = "";

        }

        //obsługa entera dla tłumaczenia
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

            //jeżeli panel tłumaczenia uruchomiony
            if (this.panel_sprawdzslowka.Visible == true)

                //jeżeli wciśnięty enter
                if (e.KeyCode == Keys.Enter)
                {
                    //uruchom przycisk sprawdź zapamietane wyłącz ten głupi dźwięk
                    e.SuppressKeyPress = true;
                    this.button3_pss_sprawdz_Click(null, null);
                }
        }

        //sprawdzenie testu
        private void button_test_sprawdz_Click(object sender, EventArgs e)
        {

            //wpisanie do zmiennej wybraneSlowo wybranego słowa
            string wybraneSlowo = "";
            if (this.radio_test_1.Checked == true)
                wybraneSlowo = this.radio_test_1.Text;
            else if (this.radio_test_2.Checked == true)
                wybraneSlowo = this.radio_test_2.Text;
            else if (this.radio_test_3.Checked == true)
                wybraneSlowo = this.radio_test_3.Text;
            else if (this.radio_test_4.Checked == true)
                wybraneSlowo = this.radio_test_4.Text;

            //jeżeli tłumaczono na angielski
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 1)
            {

                //jeżeli dobry wybów
                if (wybraneSlowo == this.AktualneSlowo.SlowkoEn)
                {

                    //dodanie punktów
                    if (this.AktualneSlowo.PunktyTlumaczenieNaEN < 10)
                        this.AktualneSlowo.PunktyTlumaczenieNaEN = this.AktualneSlowo.PunktyTlumaczenieNaEN + 1;

                    //wyświetlenie wyniku na labelach
                    this.label_spr_pop.ForeColor = System.Drawing.Color.Green;
                    this.label_spr_pop.Text = "OK";
                    this.Statystyki.Poprawne++;
                }
                //jeżeli zły wybór
                else
                {
                    //odjęcie punktów
                    if (this.AktualneSlowo.PunktyTlumaczenieNaEN > 0)
                        this.AktualneSlowo.PunktyTlumaczenieNaEN = this.AktualneSlowo.PunktyTlumaczenieNaEN - 1;

                    //wyświetlenie wyniku na labelach
                    this.label_spr_pop.ForeColor = System.Drawing.Color.Red;
                    this.label_spr_pop.Text = "Źle";
                    this.Statystyki.Niepoprawne++;
                }

                //aktualizacja bazy
                this.Baza.commend("UPDATE slowka SET PunktyTlumaczenieNaEN=" + this.AktualneSlowo.PunktyTlumaczenieNaEN + " WHERE SlowkoPL='" + this.AktualneSlowo.SlowkoPL + "' and SlowkoEn='" + this.AktualneSlowo.SlowkoEn + "';");
            }
            //jeżeli tłumaczono na pl
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 2)
            {

                //jeżeli wybrano dobre słówko
                if (wybraneSlowo == this.AktualneSlowo.SlowkoPL)
                {

                    //dodanie punktów
                    if (this.AktualneSlowo.PunktyTlumaczenieNaPL < 10)
                        this.AktualneSlowo.PunktyTlumaczenieNaPL = this.AktualneSlowo.PunktyTlumaczenieNaPL + 1;

                    //wyświetlenie wyników
                    this.label_spr_pop.ForeColor = System.Drawing.Color.Green;
                    this.label_spr_pop.Text = "OK";
                    this.Statystyki.Poprawne++;
                }
                //jeżeli wybrano złe słówko
                else
                {

                    //odjęcie punktów
                    if (this.AktualneSlowo.PunktyTlumaczenieNaPL > 0)
                        this.AktualneSlowo.PunktyTlumaczenieNaPL = this.AktualneSlowo.PunktyTlumaczenieNaPL - 1;

                    //wyświetlenie wyników
                    this.label_spr_pop.ForeColor = System.Drawing.Color.Red;
                    this.label_spr_pop.Text = "Źle";
                    this.Statystyki.Niepoprawne++;
                }

                //aktualizacja w bazie
                this.Baza.commend("UPDATE slowka SET PunktyTlumaczenieNaPL=" + this.AktualneSlowo.PunktyTlumaczenieNaPL + " WHERE SlowkoPL='" + this.AktualneSlowo.SlowkoPL + "' and SlowkoEn='" + this.AktualneSlowo.SlowkoEn + "';");

            }

            //wyświetlenie poprzedniego słówka
            this.label_test_last_en.Text = this.AktualneSlowo.SlowkoEn;
            this.label_test_last_pl.Text = this.AktualneSlowo.SlowkoPL;

            //aktualizacja statystyk
            this.Statystyki.ladujStatystykiAktualne();
            this.Statystyki.odswiez(this.ListaSlowek);

            //losowy kierunek tłumaczenia
            this.Kierunek_PLnaEN_ENnaPL_Random = 3;

            //nowe słówko
            this.ladujslowko();

            //losowanie błędnych odpowiedzi
            String[] oppcje = new String[4];
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 1)
            {
                oppcje[0] = this.AktualneSlowo.SlowkoEn;
                oppcje[1] = this.ListaSlowek[this.RAndom.Next(0, this.ListaSlowek.Count)].SlowkoEn;
                oppcje[2] = this.ListaSlowek[this.RAndom.Next(0, this.ListaSlowek.Count)].SlowkoEn;
                oppcje[3] = this.ListaSlowek[this.RAndom.Next(0, this.ListaSlowek.Count)].SlowkoEn;
                this.label_test_slowko.Text = this.AktualneSlowo.SlowkoPL;
                this.label_test_opis.Text = this.AktualneSlowo.OpisEN;
            }
            if (this.Kierunek_PLnaEN_ENnaPL_Random == 2)
            {
                oppcje[0] = AktualneSlowo.SlowkoPL;
                oppcje[1] = ListaSlowek[RAndom.Next(0, ListaSlowek.Count)].SlowkoPL;
                oppcje[2] = ListaSlowek[RAndom.Next(0, ListaSlowek.Count)].SlowkoPL;
                oppcje[3] = ListaSlowek[RAndom.Next(0, ListaSlowek.Count)].SlowkoPL;
                this.label_test_slowko.Text = AktualneSlowo.SlowkoEn;
                this.label_test_opis.Text = AktualneSlowo.OpisPL;
            }

            //losowe przypisanie słówek do radiobuttonów
            int z = RAndom.Next(0, 4);
            this.radio_test_1.Text = oppcje[z];
            if (z == 3) z = 0; else z++;
            this.radio_test_2.Text = oppcje[z];
            if (z == 3) z = 0; else z++;
            this.radio_test_3.Text = oppcje[z];
            if (z == 3) z = 0; else z++;
            this.radio_test_4.Text = oppcje[z];
        }

        //panel edycja słówek jeżeli wybrano słówko
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

            //obiekt zaznaczonego pola
            ListView.SelectedListViewItemCollection selectedItems = this.listView2.SelectedItems;

            //jeżeli zaznaczono 1 pole
            if (selectedItems.Count > 0 && selectedItems.Count < 2)
            {

                //obiekt wybranego słówka
                Dic wybraneSlowko = null;

                //pobranie danych procent1 wybranego pola
                foreach (ListViewItem item in selectedItems)
                    wybraneSlowko = this.ListaSlowek.Find(el => el.SlowkoPL == item.SubItems[0].Text && el.SlowkoEn == item.SubItems[1].Text);

                //utworzenie okna procent1 edycję słówka
                Edycja edycjaslowka = new Edycja(wybraneSlowko.SlowkoPL, wybraneSlowko.SlowkoEn, this, wybraneSlowko.OpisPL, wybraneSlowko.OpisEN);
                edycjaslowka.Show();
            }
        }

        //akceptacja niby źle wpisanego słówka
        private void button_pss_jednak_Click(object sender, EventArgs e)
        {

            //jeżeli było tłumaczenie na polski
            if (this.PoprzednieSlowkoENnaPL == true)
            {
                //dodaje punkty
                if (this.PoprzednieSlowko.PunktyTlumaczenieNaPL < 9)//dodaje 2 punkty
                    this.PoprzednieSlowko.PunktyTlumaczenieNaPL = this.PoprzednieSlowko.PunktyTlumaczenieNaPL + 2;
                else if (this.PoprzednieSlowko.PunktyTlumaczenieNaPL < 10)//dodaje 1 punkt
                    this.PoprzednieSlowko.PunktyTlumaczenieNaPL = this.PoprzednieSlowko.PunktyTlumaczenieNaPL + 1;

                //dodaje punkty do bazy
                this.Baza.commend("UPDATE slowka SET PunktyTlumaczenieNaPL=" + this.PoprzednieSlowko.PunktyTlumaczenieNaPL + " WHERE SlowkoPL='" + this.PoprzednieSlowko.SlowkoPL + "' and SlowkoEn='" + this.PoprzednieSlowko.SlowkoEn + "';");
            }

            // jeżeli było tłumaczenie na angielski
            else
            {
                //dodaje punkty
                if (this.PoprzednieSlowko.PunktyTlumaczenieNaEN < 9)
                    this.PoprzednieSlowko.PunktyTlumaczenieNaEN = this.PoprzednieSlowko.PunktyTlumaczenieNaEN + 2;//dodaje 2 punkty
                else if (this.PoprzednieSlowko.PunktyTlumaczenieNaEN < 10)
                    this.PoprzednieSlowko.PunktyTlumaczenieNaEN = this.PoprzednieSlowko.PunktyTlumaczenieNaEN + 2;//dodaje 1 punkt

                //dodaje punkty do bazy
                this.Baza.commend("UPDATE slowka SET PunktyTlumaczenieNaEN=" + this.PoprzednieSlowko.PunktyTlumaczenieNaEN + " WHERE SlowkoPL='" + this.PoprzednieSlowko.SlowkoPL + "' and SlowkoEn='" + this.PoprzednieSlowko.SlowkoEn + "';");
            }

            //statusbar
            this.statusbartext.Text = "Zaliczono";

            //dodaje do listy nowy obiekt
            //ListaSlowek.Add(wybraneSlowko);

            //wyłącza przycisk
            this.button_pss_jednak.Visible = false;

            //zmienia że odpowiedziałeś dobrze
            this.label_pss_poprawnosc.ForeColor = System.Drawing.Color.Green;
            this.label_pss_poprawnosc.Text = "OK";

            //do statystyk aktualnych dodaje poprawną odpowiedź slowkoPL usuwa złą
            this.Statystyki.Niepoprawne--;
            this.Statystyki.Poprawne++;

            //odświeża statystyki główne
            this.Statystyki.ladujStatystykiAktualne();
            this.Statystyki.odswiez(ListaSlowek);
        }

        //wyzerowanie wszystkich wyników bazy
        private void resetujStatystykiBazyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Baza.commend("UPDATE slowka SET PunktyTlumaczenieNaPL=0, PunktyTlumaczenieNaEN=0;");
            this.panel_loading.Visible = true;
            this.statusbartext.Text = "Wyzerowano statystyki bazy;";
        }

        //o autorze
        private void oAutorzeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Paweł Czubak\npawel33317@gmail.com\nhaks.pl", "O autorze", MessageBoxButtons.OK);
        }

        //o programie
        private void oProgramieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Program do nauki słówek, różni się tym od innych, że możemy tworzyć własne bazy ze słówkami.\n\nDwie opcje nauki 1. slowka 2. wpisywanie całych słówek\n\nProgram działa na zasadzie, że słówka które już znamy są wyświetlane rzadko slowkoPL te których jeszcze nie znamy częściej\n\n\nProgram jest w wersji beta kiedyś może dokończę zapamietane co ważne nie jest odporny na wszystkie błędy tak więc może się podczas głupiego używania wysypać.", "O programie", MessageBoxButtons.OK);
        }

        //zasady
        private void zasadyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Kiedyś opiszę jak będę miał dużo dużo wolnego czasu", "Zasady", MessageBoxButtons.OK);
        }
    }
}
