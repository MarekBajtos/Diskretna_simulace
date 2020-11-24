using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox2.Text = (trackBar4.Value).ToString();
            textBox3.Text = (trackBar5.Value).ToString();
            textBox4.Text = (trackBar6.Value).ToString();

        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            textBox2.Text = (trackBar4.Value).ToString();
            textBox3.Text = (trackBar6.Value).ToString();
            textBox4.Text = (trackBar5.Value).ToString();

            int PocetStolov = trackBar4.Value;
            int PocetObsluha = trackBar6.Value;
            int PocetKuchar = trackBar5.Value;

            int MaxPotrebStolov = 0, MaxPotrebObsluha = 0, MaxPotrebKuchar = 0;
            //vysledkom vypoctu bude pocet obsluzenych zakaznikov
            Model model = new Model(PocetStolov, PocetObsluha, PocetKuchar);
            model.Vypocet();

            textBox16.Text = (model.ObsluzenychLudi).ToString();
            textBox17.Text = (model.LudiaOdisli).ToString();
            textBox18.Text = (model.MaxPotrebStolov).ToString();
            textBox19.Text = (model.MaxPotrebObsluha).ToString();
            textBox20.Text = (model.MaxPotrebKuchar).ToString();
        }
    }

    public class Udalost
    {
        public int kdy;
        public Osoba kdo;
        public StavHosta akce;
        public int host;
        public Udalost(int kdy, Osoba kdo, StavHosta akce, int host)
        {
            this.kdy = kdy;
            this.kdo = kdo;
            this.akce = akce;
            this.host = host;
        }
    }

    public class Kalendar
    {
        public List<Udalost> zoznam;
        public Kalendar()
        {
            zoznam = new List<Udalost>();
        }

        public void Pridaj(int kdy, Osoba kdo, StavHosta akce, int host)
        {
            zoznam.Add(new Udalost(kdy, kdo, akce, host));
        }

        public void Odober(Osoba kdo, int host)
        {
            for (int i = 0; i < zoznam.Count; i++)
            {
                if ((zoznam[i].kdo == kdo) && (zoznam[i].host == host))
                {
                    Udalost ud = zoznam[i];
                    zoznam.Remove(ud);
                }
            }
            return; // odoberame vstky vyskyty, to je potrebne, napr. ak odchadza zakaznik, ktoreho jedlo uz kuchari pripravuju, potrebujeme zrusit vsetkych tychto kucharov
        }

        public Udalost Vyber() //Vrati udalost, ktora sa udeje ako prva v zozname udalosti.
        {
            Udalost help = null;
            foreach (Udalost udalost in zoznam)
                if ((help == null) || (udalost.kdy < help.kdy))
                    help = udalost;
            if (help != null) zoznam.Remove(help);
            return help;
        }
    }

    public enum Osoba
    {
        Host,
        Casnik,
        Kuchar
    }

    public enum StavHosta
    {
        NovyHost,
        Usadeny,
        Objednane,
        DoneseneJedlo,
        CakaNaObjednanieNapoja,
        DonesenyNapoj,
        Zaplatim,
        Odchod
    }

    public class Model
    {
        public int cas; //cas, ktorym sa bude cela simulace riadit
        public int MaxCas;
        public int MaxPocStol;
        public int MaxPocObsl;
        public int MaxPocKuch;

        public int MaxPotrebStolov;
        public int MaxPotrebObsluha;
        public int MaxPotrebKuchar;

        public int CisloZakaznika; //citac pre poradove cisla skupiny zakaznikov
        public int PocetVolStoly; //Premenne udrziavajuce informaciu o aktualnom pocte volnych stolov, casnikov

        public int ZakazniciOdisli; //Pocet zakaznikov, ktory odisli kvoli dlhemu cakaniu
        public int LudiaOdisli;
        public int ObsluzenychZakaznikov; //kolko zakaznikov sme uz obsluzili, pocitame 1 za kazdeho cloveka v skupine zakaznikov
        public int ObsluzenychLudi;

        public Kalendar KalendarRestaurace;
        public List<Zakaznik> VRestauraci; //Zoznam zakaznikov v restauraci
        public Queue<Zakaznik> FrontaNaVstupe; //Fronta zakaznikov cakajucich na vstup do restaurace 
        public Kuchyna kuchyna;
        public Obsluha obsluha;

        public Random rnd;

        public class Zakaznik
        {
            public int Cislo;
            public int PocetLudi;
            public int[] jedlo;
            public int PocetStolov; //Dopocitane z poctu ludi, usetri vypocet v inej casti programu
            public StavHosta Stav;
            public int Spokojnost;

            public Zakaznik(ref int PoradoveCislo, Random rnd)
            {
                Cislo = PoradoveCislo;
                PoradoveCislo += 1; //Zvysime citac zakaznikov v modely o 1

                //V zavislosti na vygenerovanom indexe sa urci, aka velka skupina ludi pride do restaurace
                int index = rnd.Next(1, 16); ; //Cislo od 1 do 10

                if (index <= 9) { PocetLudi = rnd.Next(1, 5); } // S pravdepodobnostou 60% bude zakaznikom skupina max 4 ludi
                else
                {
                    if (index <= 13) { PocetLudi = rnd.Next(5, 11); } //S pravdepodobnostou 30% bude zakaznikom skupina od 5 do 10 ludi
                    else { PocetLudi = rnd.Next(11, 16); }   //S pravdepodobnostou 10% bude zakaznikom skupina ludi od 11 do 15 - vacsiu skupinu neobsluhujeme          
                }

                //Console.WriteLine(" pocet " + PocetLudi);

                PocetStolov = PocetLudi / 4; //Restaurace bude mit stoly so 4 miestami
                                             //Skupinu ludi rozdelime k stolom, vzdy tak, aby sedela cela skupina, aj za predpokladu, ze nejaky clen skupiny bude pri stole sam
                if (PocetLudi % 4 > 0) { PocetStolov += 1; }

                Stav = StavHosta.NovyHost;

                jedlo = new int[4]; //jedlo v restauraci rozdelime do 3 skupin, podle casove narocnosti na pripravu

                //Nahodne generujeme, ake jedla si dana skupina ludi objedna
                jedlo[0] = rnd.Next(0, PocetLudi + 1);
                if (jedlo[0] < PocetLudi)
                {
                    jedlo[1] = rnd.Next(0, PocetLudi + 1 - jedlo[0]);
                }
                else { jedlo[1] = 0; }
                jedlo[2] = PocetLudi - jedlo[0] - jedlo[1];

                Spokojnost = 1; //Identifikator, ci bol zakaznik spokojny. 1 znamena, ze nebol spokojny. Ak sa to za behu simulace nezmeni, bude povazovany za nespokojneho
            }
        }

        public class Kuchyna
        {
            public class ObjednavkaVKuchyni
            {
                public int CisloObjednavky;
                public int[] jedlo;
                public int HotoveJedla;
                public int CelkovoObjednanych;
                public int cas;

                public ObjednavkaVKuchyni(int cislo, int[] jedlo, int cas)
                {
                    this.CisloObjednavky = cislo;
                    this.jedlo = new int[4];
                    this.jedlo[0] = jedlo[0];
                    this.jedlo[1] = jedlo[1];
                    this.jedlo[2] = jedlo[2];
                    this.jedlo = jedlo;
                    this.HotoveJedla = 0;
                    this.CelkovoObjednanych = jedlo[0] + jedlo[1] + jedlo[2];
                    this.cas = cas;
                }
            }

            public int PocetVolKuchar;
            public List<ObjednavkaVKuchyni> FrontaObjednavokVKuchyni;
            public List<ObjednavkaVKuchyni> ZoznamPripravovanychObjednavok;

            public Kuchyna(int PocetKucharov)
            {
                PocetVolKuchar = PocetKucharov;
                FrontaObjednavokVKuchyni = new List<ObjednavkaVKuchyni>();
                ZoznamPripravovanychObjednavok = new List<ObjednavkaVKuchyni>();
            }

            public void PridajObjednavkuDoKuchyne(int cislo, int[] jedlo, ref Kalendar Kalendar, int cas)
            {
                FrontaObjednavokVKuchyni.Add(new ObjednavkaVKuchyni(cislo, jedlo, cas));

                int PredchadzajuciPovetvolnychKucharov = PocetVolKuchar + 1;

                //Kedze jedna objednavka moze obsahovat viac jedal, po prichode objednavky zamestname co mozno najviac kucharov
                //Teda pokusame sa zamestnat kucharov, dokym pocet kucharov po pokuse ich zamestnat neklesne oproti predchajucemu poctu kucharov
                //To sa zastavi aj v pripade, ze nie je dost kucharov na vsetky aktualne objednavky
                while (PocetVolKuchar != PredchadzajuciPovetvolnychKucharov)
                {
                    PredchadzajuciPovetvolnychKucharov = PocetVolKuchar;
                    ZamestnajKuchara(ref Kalendar, cas);
                }
            }

            public void OdstranZakaznika(int cislo) //Pri odchode nespokojneho zakaznika zrusime jeho poziadavku na obsluhu
            {
                foreach (ObjednavkaVKuchyni objednavka in ZoznamPripravovanychObjednavok)
                { //Vyradenie daneho zakaznika z pripravovanych objednavok
                    if (objednavka.CisloObjednavky == cislo)
                    {
                        ZoznamPripravovanychObjednavok.Remove(objednavka);
                        return; // odoberame len prvy vyskyt
                    }
                }

                foreach (ObjednavkaVKuchyni objednavka in FrontaObjednavokVKuchyni)
                { // Vyrdenie daneho zakaznika z fronty objednavok, ktore sa este nezacali pripravovat
                    if (objednavka.CisloObjednavky == cislo)
                    {
                        FrontaObjednavokVKuchyni.Remove(objednavka);
                        return; // odoberame len prvy vyskyt
                    }
                }
            }

            public void OdoberZKalendara(ref Kalendar Kalendar, int cislo, int cas)
            {
                for (int i = 0; i < Kalendar.zoznam.Count; i++)
                {
                    if ((Kalendar.zoznam[i].kdo == Osoba.Kuchar) && (Kalendar.zoznam[i].host == cislo))
                    {
                        Udalost ud = Kalendar.zoznam[i];
                        Kalendar.zoznam.Remove(ud);
                        PocetVolKuchar += 1; //odstranili sme z kalendara zamestnanych kucharov, pracujucich na objednavke, pre zakaznika, ktory odchadza
                        ZamestnajKuchara(ref Kalendar, cas);
                    }
                }
                return;
            }

            public void ZamestnajKuchara(ref Kalendar Kalendar, int cas)
            {
                int Pom = -1;
                int i = 0;

                if (PocetVolKuchar > 0)
                { // Najprv dokoncime rozpracovanu objednavku. Ak su vsetky jedla z rozpracovanych objednavom uz v priprave, volny kuchar zacne pracovat na novej objednavke
                    for (int j = 0; j < ZoznamPripravovanychObjednavok.Count; j++) //Zistujeme, ci existuje rozpracovana objednavka, ktoru treba dokoncit
                    {
                        if (ZoznamPripravovanychObjednavok[j].jedlo[0] + ZoznamPripravovanychObjednavok[j].jedlo[1] + ZoznamPripravovanychObjednavok[j].jedlo[2] > 0)
                        {
                            Pom = j;
                        }
                    }

                    if (Pom >= 0)
                    { //V zozname pripravovanych objednavok je este objednavka, ktoru treba dokoncit
                        i = 2;
                        while (ZoznamPripravovanychObjednavok[Pom].jedlo[i] == 0) { i -= 1; }
                        ZoznamPripravovanychObjednavok[Pom].jedlo[i] -= 1;
                        switch (i)
                        { //podla narocnosti jedla, zamestname jedneho kuchara pripravou jedneho jedla z danej objednavky
                            case 0: //Jedla na pozicii 0 trva pripravit 4 minuty
                                Kalendar.Pridaj(cas + 4, Osoba.Kuchar, StavHosta.Objednane, ZoznamPripravovanychObjednavok[Pom].CisloObjednavky);
                                break;
                            case 1: //Jedla na pozicii 1 trva pripravit 7 minut
                                Kalendar.Pridaj(cas + 7, Osoba.Kuchar, StavHosta.Objednane, ZoznamPripravovanychObjednavok[Pom].CisloObjednavky);
                                break;
                            case 2: //Jedla na pozicii 2 trva pripravit 10 minut
                                Kalendar.Pridaj(cas + 10, Osoba.Kuchar, StavHosta.Objednane, ZoznamPripravovanychObjednavok[Pom].CisloObjednavky);
                                break;
                            default:
                                break;
                        }
                        PocetVolKuchar -= 1;
                    }
                    else //Ziadna objednavka uz nie je rozpracovana, teda vezmeme dalsiu objednavku z fronty a zacneme na nej pracovat
                    {
                        if (FrontaObjednavokVKuchyni.Count > 0)
                        {
                            //Najdeme objednavku, ktora prisla najskor
                            ObjednavkaVKuchyni pomocna = null;
                            foreach (ObjednavkaVKuchyni objednavka in FrontaObjednavokVKuchyni)
                                if ((pomocna == null) || (objednavka.cas < pomocna.cas))
                                    pomocna = objednavka;
                            FrontaObjednavokVKuchyni.Remove(pomocna); //Odstranime objednavku z fronty na pripravu

                            i = 2; //Najdeme jedlo, ktore treba pripravit, od casovo narocnejich po menej narocne
                            while (pomocna.jedlo[i] == 0) { i -= 1; }
                            pomocna.jedlo[i] -= 1;

                            switch (i)
                            { //podla narocnosti jedla, zamestname jedneho kuchara pripravou jedneho jedla z danej objednavky
                                case 0: //Jedla na pozicii 0 trva pripravit 4 minuty
                                    Kalendar.Pridaj(cas + 4, Osoba.Kuchar, StavHosta.Objednane, pomocna.CisloObjednavky);
                                    break;
                                case 1:  //Jedla na pozicii 1 trva pripravit 7 minut
                                    Kalendar.Pridaj(cas + 7, Osoba.Kuchar, StavHosta.Objednane, pomocna.CisloObjednavky);
                                    break;
                                case 2: //Jedla na pozicii 2 trva pripravit 10 minut
                                    Kalendar.Pridaj(cas + 10, Osoba.Kuchar, StavHosta.Objednane, pomocna.CisloObjednavky);
                                    break;
                                default:
                                    break;
                            }

                            ZoznamPripravovanychObjednavok.Add(pomocna); //Do zoznamu pridame objednavku, kde uz oznacime, ktore jedlo ide dany kuchar pripravit

                            PocetVolKuchar -= 1; // Jeden kuchar sa zamestna varenim
                        }
                    }
                }
            }

            public void SpracujHotoveJedlo(ref Kalendar kalendar, int cislo, int cas) //Jeden kuchar dovaril jeden chod z objednavky
            {
                foreach (ObjednavkaVKuchyni objednavka in ZoznamPripravovanychObjednavok)
                { //Vyradenie daneho zakaznika z pripravovanych objednavok
                    if (objednavka.CisloObjednavky == cislo)
                    {
                        objednavka.HotoveJedla += 1; //Jedno jedlo z danej objednavky je hotove
                        PocetVolKuchar += 1; //Kuchar, ktory varil dane jedlo sa uvolnil
                        ZamestnajKuchara(ref kalendar, cas); //Uvolneneho kuchara mozeme zas zamestnat
                        if (objednavka.HotoveJedla == objednavka.CelkovoObjednanych) //Objednavka je hotova, moze sa odniest hostom
                        {
                            kalendar.Pridaj(cas, Osoba.Casnik, StavHosta.Objednane, objednavka.CisloObjednavky); //Dame pokyn casnikovy, ze jedlo je hotove a moze ho odniest zakaznikom
                            ZoznamPripravovanychObjednavok.Remove(objednavka); //Hotovu objednavku vyradime z kuchyne                            
                        }

                        return; // odoberame len prvy vyskyt
                    }
                }
            }
        }

        public class Obsluha
        {
            public class UlohaPreObsluhu
            {
                public int cas;
                public int cisloHosta;

                public UlohaPreObsluhu(int cas, int cisloHosta)
                {
                    this.cas = cas;
                    this.cisloHosta = cisloHosta;
                }
            }

            public List<UlohaPreObsluhu> ZoznamUlohPreObsluhu;
            public int PocetVolObsluha;

            public Obsluha(int MaxPocObsl)
            {
                this.ZoznamUlohPreObsluhu = new List<UlohaPreObsluhu>();
                this.PocetVolObsluha = MaxPocObsl;
            }

            public void SpracujUlohu(int cas, int cisloHosta, ref Kalendar kalendar, ref List<Zakaznik> VRestauraci, ref Kuchyna kuchyna)
            {
                if (cisloHosta == -1) //Identifikace uvolneni casnika
                {
                    PocetVolObsluha += 1;
                }
                else //Prisla nova uloha pre obsluhu
                {
                    ZoznamUlohPreObsluhu.Add(new UlohaPreObsluhu(cas, cisloHosta));
                }

                ZamestnajObsluhu(ref kalendar, ref VRestauraci, ref kuchyna, cas);
            }

            public int NajdiStolSoZakaznikom(int cislo, List<Zakaznik> VRestauraci)
            { //Vrati pozici zakaznika s danym cislem v seznamu VRestauraci, ak se nenajse vrati -1;
                int help = -1;
                for (int i = 0; i < VRestauraci.Count; i++)
                {
                    if (VRestauraci[i].Cislo == cislo) { help = i; }
                }
                return help;
            }

            public void OdstranZakaznika(int cislo) //Pri odchode nespokojneho zakaznika zrusime jeho poziadavku na obsluhu
            {
                foreach (UlohaPreObsluhu uloha in ZoznamUlohPreObsluhu)
                {
                    if (uloha.cisloHosta == cislo)
                    {
                        ZoznamUlohPreObsluhu.Remove(uloha);
                        return; // odoberame len prvy vyskyt
                    }
                }
            }

            public void OdoberZKalendara(ref Kalendar Kalendar, int cislo, ref List<Zakaznik> VRestauraci, ref Kuchyna kuchyna, int cas)
            {
                foreach (Udalost ud in Kalendar.zoznam)
                {
                    if ((ud.kdo == Osoba.Casnik) && (ud.host == cislo))
                    {
                        Kalendar.zoznam.Remove(ud);
                        PocetVolObsluha += 1; //odstranili sme z kalendara ulohu pre casnika zadanu zakaznikom, ktory odchadza 
                        ZamestnajObsluhu(ref Kalendar, ref VRestauraci, ref kuchyna, cas); //Uvolneneho casnika skusime zamestnat
                    }
                }
                return;
            }

            public void ZamestnajObsluhu(ref Kalendar KalendarRestaurace, ref List<Zakaznik> VRestauraci, ref Kuchyna kuchyna, int cas)
            {
                if (PocetVolObsluha > 0) // Ak je volny casnik, zamestname ho 
                {
                    //Vyhladanie ulohy, ktora sa musi spracovat nejdriv. Ak nemame ziadnu ulohu, nic sa nestane.
                    UlohaPreObsluhu NaSpracovanie = null;
                    foreach (UlohaPreObsluhu uloha in ZoznamUlohPreObsluhu)
                        if ((NaSpracovanie == null) || (uloha.cas < NaSpracovanie.cas))
                            NaSpracovanie = uloha;

                    if (NaSpracovanie != null) //Ak sa nasla uloha pre obsluhu, tak ta, ktoru treba spracovat nejdriv
                    {
                        ZoznamUlohPreObsluhu.Remove(NaSpracovanie); //Odstranenie ulohy zo zoznamu uloh

                        int Pozice = NajdiStolSoZakaznikom(NaSpracovanie.cisloHosta, VRestauraci); //Pozice zakaznika v zozname VRestauraci

                        if (Pozice == -1) //Host s danym cislem se nenasel v restauraci
                        {
                            Console.WriteLine("CHYBA. Nenasiel sa host s danym cislom, medzi hostami usadenymi v restauraci, chyba moze byt v zruseni udalosti v kalendari, ak hostovy dojde trpezlivost na cakanie na casnika.");
                        }
                        else //Host s danym cislom sa nasiel v restauraci
                        {
                            KalendarRestaurace.Odober(Osoba.Host, VRestauraci[Pozice].Cislo); //Odoberieme z kalendara zakaznikovu trpezlivost
                            PocetVolObsluha -= 1;

                            int CasNaPripravuNapojov;
                            switch (VRestauraci[Pozice].Stav)
                            {
                                case StavHosta.Usadeny: //Usadeny host si chce objednat, zamestna 1 casnika                                    
                                    kuchyna.PridajObjednavkuDoKuchyne(VRestauraci[Pozice].Cislo, VRestauraci[Pozice].jedlo, ref KalendarRestaurace, cas); //Predanie objednavky do kuchyne, to da pokyn kucharom, aby varili
                                    VRestauraci[Pozice].Stav = StavHosta.Objednane;

                                    //Uvolnenie casnika po tom, co prijme objednavku a donesie zakaznikovi napoje
                                    CasNaPripravuNapojov = VRestauraci[Pozice].PocetLudi; //Pre kazdeho zakaznika bude trvat priprava napoja pol minuty
                                    KalendarRestaurace.Pridaj(cas + CasNaPripravuNapojov + 3, Osoba.Casnik, StavHosta.Objednane, -1); //Casnika uvolnime za 3 min (cas na prijatie objednavky) + pol minuty za kazdeho hosta na pripravu napoja

                                    //Trpezlivost zakaznika na prinesenie jedla, po objednani a prineseni napojov
                                    int CasNaCakanieNaJedlo = VRestauraci[Pozice].PocetStolov * 10 + 15; //Trpezlivost na pripravu jedla bude 15 min + 10 min za kazdy stol daneho zakaznika (viac ludi bude musiet dlhsie cakat na pripravu)
                                    KalendarRestaurace.Pridaj(cas + CasNaPripravuNapojov + 3 + CasNaCakanieNaJedlo, Osoba.Host, StavHosta.Objednane, VRestauraci[Pozice].Cislo);
                                    break;
                                case StavHosta.Objednane: //Jedlo je hotove, kuchyna dala pokyn na odnesenie
                                    VRestauraci[Pozice].Stav = StavHosta.DoneseneJedlo;

                                    KalendarRestaurace.Pridaj(cas + 4, Osoba.Casnik, StavHosta.DoneseneJedlo, -1); //Uvolnenie casnika po 2 min, kedy nosi jedlo na stol
                                    KalendarRestaurace.Pridaj(cas + 4 + 15, Osoba.Host, StavHosta.DoneseneJedlo, VRestauraci[Pozice].Cislo); //15 min zakaznik je
                                    break;
                                case StavHosta.CakaNaObjednanieNapoja:
                                    VRestauraci[Pozice].Stav = StavHosta.DonesenyNapoj;

                                    CasNaPripravuNapojov = VRestauraci[Pozice].PocetLudi; //Pre kazdeho zakaznika bude trvat priprava napoja pol minuty
                                    KalendarRestaurace.Pridaj(cas + 3 + CasNaPripravuNapojov, Osoba.Casnik, StavHosta.DonesenyNapoj, -1); //Uvolnenie casnika po 1 min na prijatie objednavky + priprava napojov
                                    KalendarRestaurace.Pridaj(cas + 3 + CasNaPripravuNapojov + 15, Osoba.Host, StavHosta.DonesenyNapoj, VRestauraci[Pozice].Cislo); //15 min na vypitie napojov                                
                                    break;
                                case StavHosta.Zaplatim:
                                    VRestauraci[Pozice].Spokojnost = 0;
                                    VRestauraci[Pozice].Stav = StavHosta.Odchod;
                                    KalendarRestaurace.Pridaj(cas + 4, Osoba.Host, StavHosta.Odchod, VRestauraci[Pozice].Cislo);
                                    KalendarRestaurace.Pridaj(cas + 4, Osoba.Casnik, StavHosta.Odchod, -1);
                                    break;
                                case StavHosta.Odchod: //Odchod nespokojneho zakaznika z restaurace po uklideni
                                    KalendarRestaurace.Pridaj(cas + 4, Osoba.Host, StavHosta.Odchod, VRestauraci[Pozice].Cislo);
                                    KalendarRestaurace.Pridaj(cas + 4, Osoba.Casnik, StavHosta.Odchod, -1);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        public Model(int stoly, int obsluha, int kuchar)
        {
            this.cas = 0;
            /*Restaurace bude otvorena 8 hodin, teda po 8 hodinach prestaneme prijimat
            objednavky. Hostia, ktori su este v restauraci dojedia, zaplatia a tym sa den skonci.*/
            this.MaxCas = 8 * 60;
            this.CisloZakaznika = 1;
            this.ZakazniciOdisli = 0;
            this.LudiaOdisli = 0;
            this.ObsluzenychZakaznikov = 0;
            this.ObsluzenychLudi = 0;

            this.MaxPocStol = stoly;
            this.MaxPocObsl = obsluha;
            this.MaxPocKuch = kuchar;

            this.MaxPotrebKuchar = 0;
            this.MaxPotrebObsluha = 0;
            this.MaxPotrebStolov = 0;

            this.VRestauraci = new List<Zakaznik>(); // seznam zakaznikov usadenych v restauracis
            this.FrontaNaVstupe = new Queue<Zakaznik>(); //vytvorenie fronty zakaznikov cakajucich na vstup do restaurace
            this.KalendarRestaurace = new Kalendar(); //vytvorenie kalendaru udalosti v restauraci
            this.kuchyna = new Kuchyna(MaxPocKuch);
            this.obsluha = new Obsluha(MaxPocObsl);

            this.PocetVolStoly = MaxPocStol;

            this.rnd = new Random();
        }

        public void GenerujNovehoHosta(ref int PoradoveCislo)
        {
            FrontaNaVstupe.Enqueue(new Zakaznik(ref PoradoveCislo, rnd)); //Prida do fronty noveho hosta
            UsadHosta(); //skusime zakaznika usadit
        }

        public void UsadHosta() //Usadenie novych hosti do restaurace
        {
            int PredchadzajuciPocetStolov = PocetVolStoly + 1;

            while (PredchadzajuciPocetStolov != PocetVolStoly) // Ak su nejaky hostia cakajuci na vstup, tak ich usadzame, dokym mame volne stoly
            {
                PredchadzajuciPocetStolov = PocetVolStoly;
                if (FrontaNaVstupe.Count > 0)
                {
                    Zakaznik NaVstup = FrontaNaVstupe.Peek();
                    if (NaVstup.PocetStolov <= PocetVolStoly) //ak je pre daneho hosta dost volnych stolov, zakaznik sa usadi
                    {
                        NaVstup = FrontaNaVstupe.Dequeue();
                        NaVstup.Stav = StavHosta.Usadeny;
                        PocetVolStoly -= NaVstup.PocetStolov; //Dane stoly sa obsadia 
                        VRestauraci.Add(NaVstup); //Zakaznik sa usadi do restaurace                
                        KalendarRestaurace.Pridaj(this.cas + 3, Osoba.Casnik, StavHosta.Usadeny, NaVstup.Cislo); // Do kalendara: ze o 3 minuty si bude chciet zakaznik objednat
                        KalendarRestaurace.Pridaj(this.cas + 3 + 15, Osoba.Host, StavHosta.Usadeny, NaVstup.Cislo); //Do kalendara: trpezlivost 15 min na cekani na objednani u casnika                
                    }
                }
            }
            if (MaxPocStol - PocetVolStoly > MaxPotrebStolov) { MaxPotrebStolov = MaxPocStol - PocetVolStoly; } //udrzujeme si premennu, kolko maximalne bolo sucasne obsadenych stolov
        }

        public void ZakaznikOdchadzaZRestaurace(int cislo, int Uklid)
        {
            foreach (Zakaznik zak in VRestauraci)
            {
                if (zak.Cislo == cislo)
                {
                    if (Uklid == 1) //ak je potrebne stol uklidit, tak ho uvolnime az za niekolko minut, ked ho obsluha uklidi. Uklidit treba ak host jedol, alebo si objedn8val napoje
                    {
                        zak.Stav = StavHosta.Odchod;
                        KalendarRestaurace.Pridaj(cas, Osoba.Casnik, StavHosta.Odchod, cislo); //V aktualnom case je potrebne stol uklidit, zadame pokyn obsluhe. 
                        return;
                    }
                    else //Netreba uklid, ak uz bol zavolany casnik, ktory uklidil, alebo host odisiel driv, nez jedol.
                    {
                        if (zak.Spokojnost == 1) //Spokojnost 1 znamena, ze neboli spokojny a odisli kvuli dlhemu cakaniu, takze ich pripocitame do statistiky
                        {
                            LudiaOdisli += zak.PocetLudi;
                            ZakazniciOdisli += 1;
                        }
                        else //Zakaznik bol spokojny. Teda ostal v restauraci az do zaplaceni, zaplatil a odchadza
                        {
                            ObsluzenychLudi += zak.PocetLudi;
                            ObsluzenychZakaznikov += 1;
                        }

                        PocetVolStoly += zak.PocetStolov; //Uvolnia sa stoly
                        VRestauraci.Remove(zak); //Zakaznik odchadza z restaurace
                        if (cas < MaxCas) { UsadHosta(); } //Na uvolnene miesto usadime novych hosti
                        return; // odoberame len prvy vyskyt
                    }
                }
            }
        }

        public void Vypocet()
        {
            KalendarRestaurace.Pridaj(1, Osoba.Host, StavHosta.NovyHost, 0);  // pridame do kalendara prichod prveho hosta          

            Udalost udalost = null;

            while ((udalost = KalendarRestaurace.Vyber()) != null) //cyklus v ktorom vyberame z kalendara udalosti podla casu a spracuvavame ich 
            {
                cas = udalost.kdy; // Aktualizace casu
                //Console.Write("CAS: " + cas + " Obs.: " + ObsluzenychLudi + " Odch.: " + LudiaOdisli + " kto " + udalost.kdo + " cislo " + udalost.host + " stav " + udalost.akce);

                if (udalost.kdo == Osoba.Host)
                {
                    switch (udalost.akce)
                    {
                        case StavHosta.NovyHost: // Generovanie a vstup noveho hosta do restaurace
                            GenerujNovehoHosta(ref CisloZakaznika);
                            int Pom1 = 1; //rnd.Next(1, 11);
                            if (cas + Pom1 < MaxCas - 30) //30 min pred zaverecnou uz negenerujeme novych hosti, takze uz generovani nedame do kalendara
                            {
                                KalendarRestaurace.Pridaj(cas + Pom1, Osoba.Host, StavHosta.NovyHost, 0); //Do kalendara: generovanie noveho hosta nahodne o 1 az 10 minut
                            }
                            break;
                        case StavHosta.Usadeny: //Ak nastane tato situace, zakaznik cakal pridlho na objednanie jedla
                            //Console.WriteLine();
                            obsluha.OdstranZakaznika(udalost.host); //Odstranenie daneho hosta zo zoznamu ukolov obsluhy
                            ZakaznikOdchadzaZRestaurace(udalost.host, 0); //Nespokojny zakaznik odchadza, netreba uklidit, zakaznik si ani neobjednal
                            break;
                        case StavHosta.Objednane:  //Ak nastane tato situace, zakaznik cakal pridlho na prinesenie jedla
                            //Console.WriteLine();
                            kuchyna.OdstranZakaznika(udalost.host); //Odstranenie daneho hosta zo zoznamov v kuchyni
                            kuchyna.OdoberZKalendara(ref KalendarRestaurace, udalost.host, cas); //Odobratie rozpracovanych jedal daneho hosta 
                            obsluha.OdstranZakaznika(udalost.host); //Odstranenie daneho hosta zo zoznamu ukolov obsluhy
                            obsluha.OdoberZKalendara(ref KalendarRestaurace, udalost.host, ref VRestauraci, ref kuchyna, cas); //Odobratie volanie obsluhy kucharom na prinesenie jedla 
                            ZakaznikOdchadzaZRestaurace(udalost.host, 0); //Zakaznik odchhadza, nie je potrebne uklidit, este nejedli
                            break;
                        case StavHosta.DoneseneJedlo: //Sytuacia, kedy zakaznik doje svoje jedlo. V tomto momente sa rozodne, ci odchadza, alebo si objedna dalsi napoj
                        case StavHosta.DonesenyNapoj: //Sytuacia, kedy zakaznik dopije napoj. Zachova sa rovnako, ako po dojedeni jedla
                            //Console.WriteLine();
                            int Pozice = obsluha.NajdiStolSoZakaznikom(udalost.host, VRestauraci); //Pozice hosta v restauraci                             
                            int OdchodAleboNapoj = rnd.Next(1, 101); //nahodne cislo od 1 do 100

                            if ((cas < MaxCas) && (OdchodAleboNapoj <= 20))
                            { //Cas este nedosiahol maxima, host si moze este objednat napoj. Objedna napoj s pravdepodobnosti 20%  
                                VRestauraci[Pozice].Stav = StavHosta.CakaNaObjednanieNapoja;
                                KalendarRestaurace.Pridaj(cas, Osoba.Casnik, StavHosta.CakaNaObjednanieNapoja, udalost.host); //Zavolanie casnika na objednanie napoja
                                KalendarRestaurace.Pridaj(cas + 15, Osoba.Host, StavHosta.CakaNaObjednanieNapoja, udalost.host); //Trpezlivost na cakanie na casnika                                
                            }
                            else //Cas prekrocil max cas, teda uz neprijimame objednavky, host musi odist alebo si uz host nechce objednat
                            {
                                VRestauraci[Pozice].Stav = StavHosta.Zaplatim; //Host si uz nic neobjednava, chce odist, musi teda este zaplatit, zavola casnika
                                KalendarRestaurace.Pridaj(cas, Osoba.Casnik, StavHosta.Zaplatim, udalost.host); //Zavolanie casnika, chceme zaplatit
                                KalendarRestaurace.Pridaj(cas + 20, Osoba.Host, StavHosta.Zaplatim, udalost.host); //Trpezlivost na cakanie na zaplatenie                       
                            }
                            break;
                        case StavHosta.CakaNaObjednanieNapoja: //Vyprsala trpezlivost na cakanie na objednanie napoja, host odchadza
                        case StavHosta.Zaplatim: //Vyprsala trpezlivost na cakanie na naplatenie, odchadza
                            obsluha.OdstranZakaznika(udalost.host); //Odstranime zakaznika zo zoznamu ukolov daneho casnika
                            ZakaznikOdchadzaZRestaurace(udalost.host, 1); //Nespokojny zakaznik odchadza, zakaznik uz jedol, takze treba uklidit
                            //Console.WriteLine();
                            break;
                        case StavHosta.Odchod:
                            ZakaznikOdchadzaZRestaurace(udalost.host, 0); //Zakaznik odchadza, stol uvolnime po uklideni
                            //Console.WriteLine();
                            break;
                        default:
                            break;
                    }
                }
                else if (udalost.kdo == Osoba.Casnik)
                {
                    //Console.WriteLine();
                    obsluha.SpracujUlohu(cas, udalost.host, ref KalendarRestaurace, ref VRestauraci, ref kuchyna);
                    if (MaxPocObsl - obsluha.PocetVolObsluha > MaxPotrebObsluha) { MaxPotrebObsluha = MaxPocObsl - obsluha.PocetVolObsluha; }
                }
                else //udalost.kdo == Osoba.Kuchar
                { //tato moznost nastane iba ked kuchar dovari nejake jedlo
                    //Console.WriteLine();
                    //Najprv zistime ci nie je zmena v max potrebnom pocte kucharov, lebo dalej sa kuchar uvolni
                    if (MaxPocKuch - kuchyna.PocetVolKuchar > MaxPotrebKuchar) { MaxPotrebKuchar = MaxPocKuch - kuchyna.PocetVolKuchar; }

                    kuchyna.SpracujHotoveJedlo(ref KalendarRestaurace, udalost.host, cas);
                }
            }
            return;
        }
    }
}