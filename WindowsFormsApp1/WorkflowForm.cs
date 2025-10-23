using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using RulesEngine;
using RulesEngine.Models;

namespace WindowsFormsApp1
{
    public static class MyFuncs
    {
        public static string AgeGroup(int age) => age < 18 ? "Dewasa" : "Anak";
        public static bool AgeShow(int age) => age < 18;

        public static double Diskon(double amount)
        {
            // debugging saja, boleh dihapus
            // Console.WriteLine("Diskon called: " + amount);
            if (amount >= 1000) return 0.15;
            if (amount >= 500) return 0.10;
            return 0;
        }
    }

    public partial class WorkflowForm : Form
    {
        private RulesEngine.RulesEngine _rulesEngine;

        public WorkflowForm()
        {
            InitializeComponent();
            LoadRulesFromJson();
        }

        private void LoadRulesFromJson()
        {
            try
            {
                string jsonPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "Rules.json");

                if (!File.Exists(jsonPath))
                {
                    MessageBox.Show("Rules.json tidak ditemukan di Desktop!", "RulesEngine",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 1. Settings - daftarkan tipe custom
                var settings = new ReSettings
                {
                    CustomTypes = new[] { typeof(MyFuncs) }
                };

                // 2. Baca & deserialize
                string json = File.ReadAllText(jsonPath);
                var workflows = JsonConvert.DeserializeObject<Workflow[]>(json);
                if (workflows == null || workflows.Length == 0)
                    throw new InvalidOperationException("Tidak ada workflow pada Rules.json");

                // 3. Inisialisasi RulesEngine - JANGAN lupa passing settings
                _rulesEngine = new RulesEngine.RulesEngine(workflows, settings);

                MessageBox.Show("RulesEngine siap (workflow: PersonRules)", "RulesEngine",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat rules JSON: {ex.Message}", "RulesEngine",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            if (_rulesEngine == null)
            {
                MessageBox.Show("RulesEngine belum siap.", "RulesEngine",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtAge.Text, out int age))
            {
                MessageBox.Show("Masukkan angka umur!");
                return;
            }

            lstResult.Items.Clear();
            var person = new Person { Age = age, Country = "ID" };

            // jalankan rule
            var results = await _rulesEngine.ExecuteAllRulesAsync("PersonRules", person);

            // tampilkan semua output sukses
            foreach (var r in results.GetFlattenedResults()
                                     .Where(x => !x.IsSuccess &&
                                                x.ActionResult?.Output != null))
                lstResult.Items.Add(r.ActionResult.Output.ToString());

            if (lstResult.Items.Count == 0)
                lstResult.Items.Add("Tidak ada rule yang match.");
        }

        #region Model
        public class Person
        {
            public int Age { get; set; }
            public string Country { get; set; } = "ID";
            public string Message { get; set; }
        }
        #endregion
    }

    // Extension method supaya GetFlattenedResults tersedia
    internal static class RuleResultExtensions
    {
        public static IEnumerable<RuleResultTree> GetFlattenedResults(
            this IEnumerable<RuleResultTree> results)
        {
            foreach (var res in results)
            {
                yield return res;
                if (res.ChildResults?.Any() == true)
                {
                    foreach (var child in res.ChildResults.GetFlattenedResults())
                        yield return child;
                }
            }
        }
    }
}