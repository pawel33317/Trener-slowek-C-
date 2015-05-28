using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Data.SQLite;
using System.IO;

namespace TrenerSlowek
{
    class Db
    {
        SQLiteConnection sqlite_conn;
        SQLiteCommand sqlite_cmd;
        String FName;

        //konstruktor
        public Db()
        {
        }

        //podaje nazwę bazy
        public String getName()
        {
            return FName.Substring(5, FName.Length - 5);
        }

        //konstruktor otwiera bazę istniejącą
        public Db(string name)
        {
            bool czyistnieje = File.Exists(name) ? true : false;
            this.FName = name;
            if (czyistnieje)
            {
                this.sqlite_conn = new SQLiteConnection("Data Source=" + name + ";Version=3;New=False;Compress=True;");
                this.sqlite_conn.Open();
                this.sqlite_cmd = sqlite_conn.CreateCommand();
            }
            else
                return;
        }

        //jeśli baza nie istnieje to ją tworzy
        //tworzy kolimny i sam plik
        public void newdb(string name)
        {
            //sprawdza czy istnieje baza jak nie to ją tworzy
            string curFile = name + ".db";
            bool czyistnieje = File.Exists(curFile) ? true : false;
            if (czyistnieje)
                return;
            else
            {

                //tworzy bazę
                this.sqlite_conn = new SQLiteConnection("Data Source=" + curFile + ";Version=3;New=True;Compress=True;");
                this.sqlite_conn.Open();
                this.sqlite_cmd = sqlite_conn.CreateCommand();
                //tworzy tabele
                this.sqlite_cmd.CommandText = "CREATE TABLE slowka (id integer primary key, SlowkoPL varchar(100), SlowkoEn varchar(100), PunktyTlumaczenieNaPL int, PunktyTlumaczenieNaEN int, OpisPL varchar(100), OpisEN varchar(100));";
                this.sqlite_cmd.ExecuteNonQuery();
                this.FName = curFile;
            }
        }

        //wykonuje komendę
        public bool commend(string com)
        {
            this.sqlite_cmd.CommandText = com;
            this.sqlite_cmd.ExecuteNonQuery();
            return true;
        }

        //wykonuje komendę i zwraca dane
        public SQLiteDataReader comSelect(string com)
        {
            this.sqlite_cmd.CommandText = com;
            return this.sqlite_cmd.ExecuteReader();
        }

        //zamyka bazę
        public bool close()
        {
            this.sqlite_conn.Close();
            return true;
        }
    }
}

