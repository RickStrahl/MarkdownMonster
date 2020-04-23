#if NETFULL

using System.Speech.Synthesis;
using System.Threading;
using System.Windows;
using Markdig;

namespace MarkdownMonster
{
    public class SpeechCommands
    {
        AppModel Model;
        private static SpeechSynthesizer _synth;

        public SpeechCommands(AppModel model)
        {
            Model = model;

            SpeakSelection();
            SpeakDocument();
            SpeakFromClipboard();
            CancelSpeak();
        }

        public CommandBase SpeakSelectionCommand { get; set; }



        void SpeakSelection()
        {
            SpeakSelectionCommand = new CommandBase((parameter, command) =>
            {
                var text = Model.ActiveEditor?.AceEditor?.GetSelection();
                if (string.IsNullOrEmpty(text))
                    return;

                text = ToPlainText(text);

                Speak(text);
            }, (p, c) => true);
        }

        public CommandBase SpeakDocumentCommand { get; set; }

        void SpeakDocument()
        {
            SpeakDocumentCommand = new CommandBase((parameter, command) =>
            {
                var text = Model.ActiveEditor?.AceEditor?.GetValue();
                if (string.IsNullOrEmpty(text))
                    return;

                text = ToPlainText(text);

                Speak(text);
            }, (p, c) => true);
        }

        

        public CommandBase SpeakFromClipboardCommand { get; set; }

        void SpeakFromClipboard()
        {
            SpeakFromClipboardCommand = new CommandBase((parameter, command) =>
            {
                if (!Clipboard.ContainsText())
                    return;

                var text = ClipboardHelper.GetText();
                text = ToPlainText(text);
                Speak(text);

            }, (p, c) => true);
        }


        public CommandBase CancelSpeakCommand { get; set; }

        void CancelSpeak()
        {
            CancelSpeakCommand = new CommandBase((parameter, command) =>
            {
                if (_synth == null)
                    return;
                _synth.SpeakAsyncCancelAll();
            }, (p, c) => true);
        }


        private void Speak(string text)
        {
            // Initialize a new instance of the SpeechSynthesizer.
            if (_synth == null)
                _synth = new SpeechSynthesizer();
            else
            {
                _synth.SpeakAsyncCancelAll();
                Thread.Sleep(100);
            }

            // Configure the audio output.
            _synth.SetOutputToDefaultAudioDevice();

            // Create a prompt from a string.
            var speak = new Prompt(text);

            // Speak the contents of the prompt synchronously.
            _synth.SpeakAsync(speak);
        }

        /// <summary>
        /// converts markdown to plain text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string ToPlainText(string text)
        {
            var parser = new MarkdownParserMarkdig(false);
            var builder = parser.CreatePipelineBuilder();
            text = Markdown.ToPlainText(text,builder.Build());

            return text;
        }


    }
}
#endif
