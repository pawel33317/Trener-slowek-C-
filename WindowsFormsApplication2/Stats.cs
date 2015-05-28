using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace TrenerSlowek
{
    class Stats
    {
        Glowne OknoGlowne;
        List<Dic> ListaSlowek;
        public int Poprawne = 0;
        public int Niepoprawne = 0;

        public Stats(Glowne oknoGlowne, List<Dic> listaSlowek)
        {
            this.OknoGlowne = oknoGlowne;
            this.ListaSlowek = listaSlowek;

            //odświeża statystyki
            this.ladujStatystykiGlowne();
            this.ladujStatystykiAktualne();
        }

        public void odswiez(List<Dic> listaSlowek)
        {
            this.ListaSlowek = listaSlowek;
            this.ladujStatystykiGlowne();
        }

        //odświeża statystyki główne
        public void ladujStatystykiGlowne()
        {

            //wszystkie słówka
            this.OknoGlowne.set_stat_label_wszystkie("Wszystkie: " + this.ListaSlowek.Count.ToString());

            //dzieli na kojarzone, zapamiętane , niezapamiętane
            int zapamietane = 0, kojarzone = 0, niezapamietane = 0;
            foreach (Dic d in ListaSlowek)
            {

                //zapamiętane pl -> en
                if (d.PunktyTlumaczenieNaEN > 8)
                    zapamietane++;

                //zapamiętane en-> pl
                if (d.PunktyTlumaczenieNaPL > 8)
                    zapamietane++;

                //kojarzone pl -> en
                if (d.PunktyTlumaczenieNaEN > 1 && d.PunktyTlumaczenieNaEN < 9)
                    kojarzone++;

                //kojarzone en -> pl
                if (d.PunktyTlumaczenieNaPL > 1 && d.PunktyTlumaczenieNaPL < 9)
                    kojarzone++;

                //niezapamiętane pl -> en
                if (d.PunktyTlumaczenieNaEN < 2)
                    niezapamietane++;

                //niezapamiętane en -> pl
                if (d.PunktyTlumaczenieNaPL < 2)
                    niezapamietane++;
            }

            //wyświetla statystyki
            this.OknoGlowne.set_stat_label_zapamietane("Zapamiętane: " + zapamietane.ToString());
            this.OknoGlowne.set_stat_label_kojarzone("Kojarzone: " + kojarzone.ToString());
            this.OknoGlowne.set_stat_label_niezapamietane("Niezapamiętane: " + niezapamietane.ToString());

            //oblicza procenty dla progresBara i wyświetla
            int procent1 = 0, procent2 = 0;
            if (zapamietane != 0)
                procent1 = Convert.ToInt32((zapamietane * 100 / (zapamietane + kojarzone + niezapamietane)));
            if (kojarzone != 0)
                procent2 = Convert.ToInt32((kojarzone * 100 / (zapamietane + kojarzone + niezapamietane)) / 2);
            this.OknoGlowne.set_stat_progressBar_db(procent1 + procent2);
        }

        //odświeża statystyki aktualne
        public void ladujStatystykiAktualne()
        {

            //wyświetla statystyki
            this.OknoGlowne.set_stat_label_poprawne("Poprawne: " + this.Poprawne.ToString());
            this.OknoGlowne.set_stat_label_niepoprawne("Niepoprawne: " + this.Niepoprawne.ToString());

            //oblicza procenty dla progresBara i wyświetla
            int z = 0;
            if (this.Poprawne == 0)
                z = 0;
            else if (this.Niepoprawne == 0)
                z = 100;
            else
                z = Convert.ToInt32((this.Poprawne * 100 / (this.Poprawne + this.Niepoprawne)));
            this.OknoGlowne.set_stat_progressBar(z);
        }

        //zeruje statystyki aktualne
        public void resetujaktualne()
        {
            this.Poprawne = 0;
            this.Niepoprawne = 0;
            this.OknoGlowne.set_stat_label_poprawne("Poprawne: " + this.Poprawne.ToString());
            this.OknoGlowne.set_stat_label_niepoprawne("Niepoprawne: " + this.Niepoprawne.ToString());
            this.OknoGlowne.set_stat_progressBar(0);
        }

        //wyświetla nazwę bazy
        public void nazwabazy(String a)
        {
            this.OknoGlowne.set_stat_label_baza(a);
        }
    }
}
