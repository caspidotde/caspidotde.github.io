<!-- Speech-To-Text auf Deutsch -->

##### Speech-To-Text auf Deutsch

- Whisper.NET (https://github.com/sandrohanea/whisper.net)
- FFMpegCore (https://github.com/rosenbjerg/FFMpegCore)
- NAudio (https://github.com/naudio/NAudio)


````csharp

#:property AssemblyName=WhisperMeTheTextOf
#:property BuiltInComInteropSupport=true
#:property PublishAot=false
#:property ErrorOnDuplicatePublishOutputFiles=false

#:package FFMpegCore@5.4.0
#:package NAudio@2.2.1
#:package Whisper.net.AllRuntimes@1.9.0

using FFMpegCore;
using FFMpegCore.Enums;
using NAudio.Wave;
using Whisper.net;

if (args.Length < 3)
{
    Console.WriteLine("Bitte den Pfad zur Video- oder Audio-Datei angeben.");
	Console.WriteLine();
    Console.WriteLine("*********************************************************************");
    Console.WriteLine("    Usage: dotnet run whisper.cs <de|en> <1-10> pathToMediaFile      ");
	Console.WriteLine("    1: ggml-large-v3                            3.0 GB               ");
	Console.WriteLine("    2: ggml-large-v2-q8_0                       1.6 GB               ");
	Console.WriteLine("    3: ggml-large-v3-turbo                      1.5 GB               ");
	Console.WriteLine("    4: whisper-large-v3-turbo-german-ggml       1.5 GB               ");
	Console.WriteLine("    5: ggml-medium-GermanMed-full-f16           1.5 GB               ");
	Console.WriteLine("    6: ggml-large-v3-q5_0                       1.0 GB               ");
	Console.WriteLine("    7: ggml-large-v3-turbo-q8_0                 850 MB               ");
	Console.WriteLine("    8: ggml-medium-q8_0                         800 MB               ");
	Console.WriteLine("    9: ggml-tiny-german-1224-f16                 75 MB               ");
	Console.WriteLine("   10: ggml-tiny-german-f16                      75 MB               ");
    Console.WriteLine("*********************************************************************");
    return;
}

var language = args[0];
if (!language.ToLower().Equals("de")) language = "en";

var modelNum = args[1];
var model = modelNum.ToLower() switch
{
    "1"  => "ggml-large-v3.bin",
    "2"  => "ggml-large-v2-q8_0.bin",
	"3"  => "ggml-large-v3-turbo.bin",
	"4"  => "whisper-large-v3-turbo-german-ggml.bin",
	"5"  => "ggml-medium-GermanMed-full-f16.bin",
	"6"  => "ggml-large-v3-q5_0.bin",
	"7"  => "ggml-large-v3-turbo-q8_0.bin",
	"8"  => "ggml-medium-q8_0.bin",
	"9"  => "ggml-tiny-german-1224-f16.bin",
	// "10" => "ggml-tiny-german-f16.bin",
    _ => "ggml-tiny-german-f16.bin"
};

var mediaFilePath = args[2];

if (!File.Exists(mediaFilePath))
{
    Console.WriteLine($"Die Datei '{mediaFilePath}' existiert nicht.");
    return;
}

Console.WriteLine("Datei gefunden: " + mediaFilePath);
var mediaInfo = await FFProbe.AnalyseAsync(mediaFilePath);
Console.WriteLine($"Dauer: {mediaInfo.Duration}");

Console.WriteLine("Sprache: " + language);
Console.WriteLine("Modell: " + model);

// Erstellen eines temporären Pfads für die extrahierte Audiodatei
var tempAudioFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");

try
{
    FFMpegArguments.FromFileInput(mediaFilePath)
    .OutputToFile(tempAudioFilePath, true, options => options
        .DisableChannel(Channel.Video)
        // .WithAudioBitrate(AudioQuality.VeryHigh)
        .ForceFormat("wav")
        .WithAudioSamplingRate(16000))
    .ProcessSynchronously();

    Console.WriteLine("Audiodatei extrahiert: " + tempAudioFilePath);

    using var tempAudioFileReader = new WaveFileReader(tempAudioFilePath);
    var totalDuration = tempAudioFileReader.TotalTime;
    Console.WriteLine($"Dauer der Audiodatei: {totalDuration}");
    // Festlegen der Segmentdauer: bei 30 Sekunden läuft jeder Vorgang schneller.
    var segmentDuration = TimeSpan.FromSeconds(30);
    // Berechnen der Anzahl der Segmente basierend auf der Gesamtdauer und der Segmentdauer
    var segmentCount = (int)Math.Ceiling(totalDuration.TotalSeconds / segmentDuration.TotalSeconds);
    Console.WriteLine($"Anzahl der Segmente: {segmentCount}");
    
    string ggmlPath = "../../Data/Ggml";

	// string filePath = $"{ggmlPath}/ggml-large-v3.bin";
    // string filePath = $"{ggmlPath}/ggml-large-v3-turbo.bin";    
    // string filePath = $"{ggmlPath}/whisper-large-v3-turbo-german-ggml.bin";
	string filePath = $"{ggmlPath}/{model}";

    // Initialisieren des Whisper-Prozessors mit der angegebenen Modell-Datei und Spracheinstellung
    using var whisperFactory = WhisperFactory.FromPath(filePath);
    using var processor = whisperFactory.CreateBuilder().WithLanguage(language).Build();

    foreach (var i in Enumerable.Range(0, segmentCount))
    {
        using var segmentReader = new WaveFileReader(tempAudioFilePath);

        var segment = segmentReader.ToSampleProvider()
            .Skip(i * segmentDuration)
            .Take(segmentDuration);

        var segmentProvider = segment.ToWaveProvider16();
        using var segmentStream = new MemoryStream();
        WaveFileWriter.WriteWavFileToStream(segmentStream, segmentProvider);

        segmentStream.Position = 0;
        var durationOffset = TimeSpan.FromSeconds(i * segmentDuration.TotalSeconds);

        await foreach (var result in processor.ProcessAsync(segmentStream))
        {
            var startTime = result.Start + durationOffset;
            var endTime = result.End + durationOffset;
            Console.WriteLine($"segment {i:d3} [{startTime:hh\\:mm\\:ss} - {endTime:hh\\:mm\\:ss}]: {result.Text}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Fehler bei der Verarbeitung der Datei: " + ex.Message);
}
finally
{
    // Bereinigen der temporären Audiodatei
    if (File.Exists(tempAudioFilePath))
    {
        try
        {
            File.Delete(tempAudioFilePath);
            Console.WriteLine("Temporäre Audiodatei gelöscht.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fehler beim Löschen der temporären Audiodatei: " + ex.Message);
        }
    }
}


````

und so sieht es aus

````
Datei gefunden: ..\Herbert Grönemeyer - Männer.mp3
Dauer: 00:04:00.2850000
Audiodatei extrahiert: C:\Users\Zoey\AppData\Local\Temp\9f31598e-fc00-4da5-bc95-cff1fd64af52.wav
Dauer der Audiodatei: 00:04:00.2743125
Anzahl der Segmente: 9
segment 000 [00:00:00 - 00:00:19]:  Männer nehmen den Arm, Männer geben Geborgenheit, Männer weinen heimlich, Männer brauchen viel Zärtlichkeit und Männer sind so verletzlich.
segment 000 [00:00:21 - 00:00:30]:  Männer sind auf dieser Welt einfach unersetzlich. Männer kaufen Frauen, Männer
segment 001 [00:00:30 - 00:00:40]:  Männer stehen ständig unter Strom, Männer baggern wie blöde, Männer lügen am Telefon und Männer sind allzeit bereit.
segment 001 [00:00:40 - 00:00:49]:  Männer bestechen durch ihr Geld und ihre Lässigkeit, Männer haben Schweningsleicht,
segment 001 [00:00:51 - 00:01:00]:  aussen hart und hirnend, ganz weich, werden als Kind schon auf mangeacht.
segment 002 [00:01:00 - 00:01:18]:  Wann ist der Mann ein Mann? Wann ist der Mann ein Möhr?
segment 002 [00:01:18 - 00:01:30]:  Männer haben Muskeln, Männer sind furcht und stark, Männer können alles, Männer kriegen einen Herzinfarkt und Männer sind ein.
segment 003 [00:01:30 - 00:01:41]:  Einsamen Streit müssen durch jede Wand, müssen immer weiter, Männer haben schwerdings leicht,
segment 003 [00:01:41 - 00:01:51]:  aus dem Haar, rot und dernig ganz weich werden als Kind schon auf Mann geeicht.
segment 003 [00:01:52 - 00:02:00]:  Wann ist ein Mann ein Mann? Wann ist ein Mann ein Mann?
segment 004 [00:02:00 - 00:02:05]:  Wann ist denn man ein Mann?
segment 004 [00:02:05 - 00:02:29]:  Männer führen Kriege, Männer sind schon als Babyblau, Männer rauchen Pfeife, Männer sind furchtbar schlau, Männer bauen Arquete, Männer machen alles der Gaffee.
segment 005 [00:02:30 - 00:02:35]:  Auf wann ist der Mann ein Möhr?
segment 005 [00:02:35 - 00:02:40]:  Auf wann ist der Mann ein Möhr?
segment 005 [00:02:40 - 00:02:58]:  Männer kriegen keine Kinder, Männer kriegen dünnes Haar, Männer sind auch Menschen, Männer sind etwas zunderbar, Männer sind so Verletzte.
segment 006 [00:03:00 - 00:03:08]:  Männer sind auf dieser Welt einfach unersetzlich, Männer haben schwerliebensweicht,
segment 006 [00:03:08 - 00:03:18]:  aussen hart und denen ganz weich, werden als Kind schon auf Mann geeicht.
segment 006 [00:03:18 - 00:03:21]:  Wann ist der Mann ein Mann?
segment 006 [00:03:21 - 00:03:26]:  Wann ist der Mann ein Möhr?
segment 007 [00:03:30 - 00:03:32]:  Auf wann ist man ein Mann?
segment 008 [00:04:00 - 00:04:29]:  Vielen Dank.
Temporäre Audiodatei gelöscht.
````

