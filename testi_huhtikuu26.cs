using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
  public class Script
  {
    public Script()
        {
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            // KOODIN TOIMINTA:
            //
            // Koodi laajentaa valittuja OAR-rakenteita PRV-rakenteiksi (oletuksena laajennus 5 mm)
            // Lisäksi PRV-rakenteet croptataan PTV:n päältä valitulla marginaalilla (oletus 5 mm)
            // PTV kopioidaan uudeksi rakenteeksi, jottei PTV:tä vahingossakaan muokata. PTV:n kopio poistetaan lopuksi
            //
            // Alla on kolme ratkaisua #region-osioihin jaettuina. Ensimmäinen on oletuksena käytössä, muut ovat kopioitu pois käytöstä.


            // Ladataan EBP:ssä auki oleva potilas ja structure set muuttujiin
            Patient pat = context.Patient;
            StructureSet ss = context.StructureSet;

            // Sallitaan muutokset
            pat.BeginModifications();


            // Tässä alustetaan halutut PRV-rakenteet listamuotoista muokkausta varten.
            // Lista ei ole käytössä muissa ratkaisuissa

            // Luodaan tyhjä lista OAR-rakenteille
            List<string> oarList = new List<string>();

            // Lisätään listaan rakenteiden nimiä
            oarList.Add("Bladder");
            oarList.Add("Rectum");

            // Määritellään laajennus ja croppaus milleinä
            double outMarg = 5;
            double cropMarg = 5;

            // Alla olevat #regionit saa auki klikkaamalla rivinumeroiden viereen tulevista nuolista, kun hiiren vie rivinumeroiden kohdalle
            #region PTV_kopiointi
            // Poimitaan PTV, mutta tehdään siitä kopio, koska alkuperäisen PTV:n muokkaamista ei haluta riskeerata
            // Alustetaan PTV-muuttuja, ja kopioidaan PTV siihen
            string copyName = "z_PTVcopy";
            Structure ptv_copy = ss.AddStructure("PTV", copyName);  // Tästä tallentuvaa muuttujaa voidaan muokata

            // Kerrotaan PTV:n nimi
            string ptvname = "PTV1";

            // Käydään kaikki structuret läpi ja etsitään rakenne PTV:n nimellä

            // .Where-komennon jälkeen tulee Lambda-lause
            //      Lambda-lause:   Muuttuja annetaan johonkin käsittelyyn, josta palautuu totuusarvo
            //                      Tässä muuttujaa s käsitellään kysymällä ensin "Id" ja totuusarvo tulee vertailusta muuttujaan "rectumName"
            //                      Totuusarvo on 1, jos s.Id = "Rectum"
            foreach (Structure s in ss.Structures.Where(s => s.Id.Equals(ptvname)))
            {
                // Kopioidaan rakenteen piirros optimointirakenteeseen, koska luotu rakenne on tyhjä
                ptv_copy.SegmentVolume = s.SegmentVolume;

                break;                      // Lähdetään pois silmukasta, kun rakenne on löytynyt
            }
            #endregion

            #region LISTAMUOTO_FUNKTIOLLA

            // Viedään lista aliohjelmaan, joka käy sen silmukassa läpi
            AddPRVStructure(oarList, ss, outMarg, ptv_copy, cropMarg);

            #endregion

            #region SUORAVIIVAINEN_MUOKKAUS

            /*
            // Luodaan optimointirakenne suoraan structure-listaan
            Structure rectumOpt = ss.AddStructure("ORGAN", "Z_Rectum+5mm 5mm crop");  // Tästä tallentuvaa muuttujaa voidaan muokata

            // Käydään kaikki structuret läpi ja etsitään rakenne nimeltä "Rectum"
            string rectumName = "Rectum";

            foreach (Structure s in ss.Structures.Where(s => s.Id.ToLower().Equals(rectumName.ToLower())))
            {
                rectumOpt.SegmentVolume = s.SegmentVolume.Margin(5).Sub(ptv_copy.Margin(5)); ;  // Kopioidaan rakenteen piirros optimointirakenteeseen, koska luotu rakenne on tyhjä


                // Kopioidaan optimointirakenteelle sama väri ja rakennetkoodi kuin alkuperäiselle
                rectumOpt.Color = s.Color;                  // Kopioidaan optimointirakenteelle sama väri kuin alkuperäiselle
                rectumOpt.StructureCode = s.StructureCode;  // Kopioidaan myös Structure code, tässä Rectum

                break;                      // Lähdetään pois silmukasta, kun rakenne on löytynyt
            }

            // Luodaan optimointirakenne suoraan structure-listaan
            Structure bladderOpt = ss.AddStructure("ORGAN", "Z_Bladder+5mm 5mm crop");  // Tästä tallentuvaa muuttujaa voidaan muokata

            // Käydään kaikki structuret läpi ja etsitään rakenne nimeltä "Bladder"
            string bladderName = "Bladder";

            foreach (Structure s in ss.Structures.Where(s => s.Id.ToLower().Equals(bladderName.ToLower())))
            {
                bladderOpt.SegmentVolume = s.SegmentVolume.Margin(5).Sub(ptv_copy.Margin(5)); ;  // Kopioidaan rakenteen piirros optimointirakenteeseen, koska luotu rakenne on tyhjä

                bladderOpt.Color = s.Color;                  // Kopioidaan optimointirakenteelle sama väri kuin alkuperäiselle
                bladderOpt.StructureCode = s.StructureCode;  // Kopioidaan myös Structure code, tässä Rectum

                break;                      // Lähdetään pois silmukasta, kun rakenne on löytynyt
            }
            */
            #endregion

            #region LISTAMUOTOINEN_MUOKKAUS
            // Rivien yli jatkuva kommentointi alkaa komennolla /*
            /*
            // Luodaan tyhjä lista OAR-rakenteille
            List<string> oarList = new List<string>();

            // Lisätään listaan rakenteiden nimiä
            oarList.Add("Bladder");
            oarList.Add("Rectum");

            // Määritellään laajennus ja croppaus milleinä
            double outMarg2 = 5;
            double cropMarg2 = 5;

            foreach (string str in oarList) {
                string newID = "Z_" + str;

                // Lisätään käytetyt marginaalit ja kroppaukset rakenteen nimeen
                string newID = "Z_" + str + "+" + outMarg2.ToString() + "mm " + cropMarg2.ToString() + "mm crop";

                Structure optStruct = ss.AddStructure("ORGAN", newID);

                // Käydään kaikki structure setin rakenteet läpi
                // string-luokan metodeja on ketjutettu:
                //      .ToLower()   = muuttaa kaikki kirjaimet pieniksi
                //      .Equals(x)   = vertaa edellistä merkkijonoa x:ään
                foreach (Structure s in ss.Structures.Where(s => s.Id.ToLower().Equals(str.ToLower() ))) {
                    
                    // Laitetaan uuden structuren piirrokseen alkuperäisen rakenteen piirros, johon
                    //      - Lisätään uuteen rakenteeseen outMarg2, jos outMarg2 > 0
                    //      - Poistetaan rakenteesta annettu rakenne, jos cropMarg2 > 0
                    optStruct.SegmentVolume = s.SegmentVolume.Margin(outMarg2).Sub( ptv_copy.Margin(cropMarg2) );

                    // Annetaan rakenteelle alkuperäisen mukainen väri ja rakennekoodi
                    optStruct.Color = s.Color;
                    optStruct.StructureCode = s.StructureCode;
                }
            }
            */
            // Rivien yli jatkuva kommentointi päättyy komennolla */
            #endregion

            // Poistetaan muokkaukseen käytetty PTV
            ss.RemoveStructure(ptv_copy);
        }

        // Aliohjelmat / apufunktiot tänne
        #region FUNKTIOT

        /// <summary>
        /// Creates a PRV margin structure and adds it to the given structure set. Inputs define an outer margin for the OAR structure and a structure <c>cropFromStructure</c> with which overlaps should be avoided, by a margin <c>cropMarg</c>. PRV structure name is auto-generated unless specified using <c>customName</c>.
        /// </summary>
        /// <param name="ss">Structure Set</param>
        /// <param name="oarID">OAR for which the PRV will be created</param>
        /// <param name="outMarg">Desired PRV margin, in mm</param>
        /// <param name="cropFromStructure">Structure for avoiding overlaps</param>
        /// <param name="cropMarg">Margin for avoiding overlaps, in mm</param>
        /// <param name="customName">Optinal: </param>
        private static void AddPRVStructure(StructureSet ss, string oarID, double outMarg, Structure cropFromStructure, double cropMarg, string customName = "")
        {
            // Sama toteutus, jossa if-lauseet ovat ketjutettu samalle riville "inline"-muodossa:
            // inline-syntaksi: ("totuustesti" ? "arvo, jos tosi" : "arvo jos epätosi")
            string newID = "Z_" + oarID + (outMarg > 0 ? "+" + outMarg.ToString() + "mm" : "") +
                                        (cropMarg > 0 ? " crop" : "");

            // Jos customName annettiin, laitetaan se newID:n tilalle
            if (!customName.Equals("")) { newID = customName; }

            // Lisätään optimointirakenne structure settiin
            Structure optStruct = ss.AddStructure("ORGAN", newID);

            // Käydään kaikki structure setin rakenteet läpi
            // string-luokan metodeja on ketjutettu:
            //      .ToLower()   = muuttaa kaikki kirjaimet pieniksi
            //      .Equals(x)   = vertaa edellistä merkkijonoa x:ään
            foreach (Structure s in ss.Structures.Where(s => s.Id.ToLower().Equals(oarID.ToLower())))
            {

                // Laitetaan uuden structuren piirrokseen alkuperäisen rakenteen piirros, johon
                //      - Lisätään uuteen rakenteeseen outMarg, jos outMarg > 0
                //      - Poistetaan rakenteesta annettu rakenne, jos cropMarg > 0
                optStruct.SegmentVolume = s.SegmentVolume.Margin(outMarg).Sub(cropFromStructure.Margin(cropMarg));

                // Annetaan rakenteelle alkuperäisen mukainen väri ja rakennekoodi
                optStruct.Color = s.Color;
                optStruct.StructureCode = s.StructureCode;
            }
        }

        /// <summary>
        /// Creates PRV margin structures for each listed structure and adds them to the given structure set. Inputs define an outer margin for the OAR structure and a structure <c>cropFromStructure</c> with which overlaps should be avoided, by a margin <c>cropMarg</c>. PRV structure name is auto-generated unless specified using <c>customName</c>.
        /// </summary>
        /// <param name="list">List of <c>Structure</c> objects.</param>
        /// <param name="ss">Structure Set</param>
        /// <param name="outMarg">Desired PRV margin, in mm</param>
        /// <param name="cropFromStructure">Structure for avoiding overlaps</param>
        /// <param name="cropMarg">Margin for avoiding overlaps, in mm</param>
        private static void AddPRVStructure(List<string> list, StructureSet ss, double outMarg, Structure cropFromStructure, double cropMarg)
        {
            foreach (string str in list) { AddPRVStructure(ss, str, outMarg, cropFromStructure, cropMarg); }
        }
        #endregion
    }
}
