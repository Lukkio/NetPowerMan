


using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Resources;
using System;
using System.Windows;
using System.Globalization;

// General Information
[assembly: AssemblyTitle("PowerManager")]
[assembly: AssemblyDescription("PowerManager")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Lucio")]
[assembly: AssemblyProduct("PowerManager")]
[assembly: AssemblyCopyright("© 2023")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: Guid("f358593f-c34f-458c-b782-98592c5afaa2")]

// Se si imposta ComVisible su false, i tipi in questo assembly non saranno visibili
// ai componenti COM. Se è necessario accedere a un tipo in questo assembly da
// COM, impostare su true l'attributo ComVisible per tale tipo.
[assembly: ComVisible(false)]

//Per iniziare a creare applicazioni localizzabili, impostare
//<UICulture>CultureYouAreCodingWith</UICulture> nel file .csproj
//all'interno di un <PropertyGroup>.  Ad esempio, se si utilizza l'inglese (Stati Uniti)
//nei file di origine, impostare <UICulture> su en-US.  Rimuovere quindi il commento dall'attributo
//NeutralResourceLanguage riportato di seguito.  Aggiornare "en-US" nella
//riga sottostante in modo che corrisponda all'impostazione UICulture nel file di progetto.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //dove si trovano i dizionari delle risorse specifiche del tema
                                     //(da usare se nella pagina non viene trovata una risorsa,
                                     // oppure nei dizionari delle risorse dell'applicazione)
    ResourceDictionaryLocation.SourceAssembly //dove si trova il dizionario delle risorse generiche
                                              //(da usare se nella pagina non viene trovata una risorsa,
                                              // nell'applicazione o nei dizionari delle risorse specifiche del tema)
)]

// Version informationr(
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: NeutralResourcesLanguageAttribute( "en-US" )]

namespace TmdbLockUpWpf
{
    public static class BuildInfo
    {
        private const long              BUILD_DATE_BINARY_UTC       = 0x48dc07ef5df01a58;    // dicembre 28, 2023 9:53:03.436245  UTC

        private static AssemblyName     BuildAssemblyName { get; }  = Assembly.GetExecutingAssembly().GetName();
        public static DateTimeOffset    BuildDateUtc { get; }       = DateTime.FromBinary(BUILD_DATE_BINARY_UTC);
        public static string            ModuleText { get; }         =  BuildAssemblyName.Name;
        public static string            VersionText { get; }        = "v" + BuildAssemblyName.Version.ToString()
#if DEBUG
                                                                                + " [DEBUG]"
#endif
                                                                                ;

        public static string            BuildDateText { get; }      = "12/28/2023 9:53  UTC";
        public static string            DisplayText { get; }        = $"{ModuleText} {VersionText} (Build Date: {BuildDateText})";
    }
}

