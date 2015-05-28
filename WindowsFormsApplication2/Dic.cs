using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrenerSlowek
{
    class Dic : IComparable<Dic>, ICloneable
    {
        public string SlowkoPL;
        public string SlowkoEn;
        public int PunktyTlumaczenieNaPL;
        public int PunktyTlumaczenieNaEN;
        public string OpisPL;
        public string OpisEN;
        public Dic(string slowkoPL, string slowkoEN, int punktyTlumaczenieNaPL, int punktyTlumaczenieNaEN, string opisPL, string opisEN)
        {
            this.SlowkoPL = slowkoPL;
            this.SlowkoEn = slowkoEN;
            this.PunktyTlumaczenieNaPL = punktyTlumaczenieNaPL;
            this.PunktyTlumaczenieNaEN = punktyTlumaczenieNaEN;
            this.OpisPL = opisPL;
            this.OpisEN = opisEN;
        }


        //do interfejsu IClonable aby skopiować obiekt
        public object Clone()
        {
            Dic tmp = new Dic(this.SlowkoPL, this.SlowkoEn, this.PunktyTlumaczenieNaPL, this.PunktyTlumaczenieNaEN, this.OpisPL, this.OpisEN);
            return tmp;
        }

        //do interfejsi IComparable aby sortować listę takich obiektów
        public int CompareTo(Dic ob)
        {
            if (this.SlowkoPL.CompareTo(ob.SlowkoPL) != 0)
                return this.SlowkoPL.CompareTo(ob.SlowkoPL);
            else
                return this.SlowkoEn.CompareTo(ob.SlowkoEn);
        }
    }
}
