using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Laba2.Runner.Infrastructure;
using Laba2.Runner.Infrastructure.TreeViewItems;
using Laba2.Runner.Utils;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace Laba2.Runner.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly OpenFileDialog _openFileDialog;

        private readonly object _lockObj = new object();

        private readonly ObservableCollection<Rule> _rules;
        private readonly ObservableCollection<Question> _questions;
        private readonly ObservableCollection<Fact> _facts;
        private Rule _currentRule;
        private Fact _currentFact;
        private Question _currentQuestion;
        private object _object;
        private ArrayList _trueFactsList;

        private string _header;


        public MainViewModel()
        {
            _openFileDialog = new OpenFileDialog();

            _rules = new ObservableCollection<Rule>();
            _questions = new ObservableCollection<Question>();
            _facts = new ObservableCollection<Fact>();

            _currentRule = new Rule();
            _currentFact = new Fact();
            _currentQuestion = new Question();

            Items = new ObservableCollection<ITreeView>();
        }

        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        public ObservableCollection<ITreeView> Items { get; set; }

        public IList<Fact> ForQuestionList
        {
            get { return _facts.Where(fact => fact.Type == FactType.ForQuestion).ToList(); }
        }

        public IList<Fact> ForResultList
        {
            get { return _facts.Where(fact => fact.Type == FactType.ForResult).ToList(); }
        }

        public ICommand OpenFileCommand => new RelayCommand(OpenFileCommandMethod);

        private void OpenFileCommandMethod()
        {
            _openFileDialog.Title = "Выберите файл с базой знаний";
            _openFileDialog.Filter = "Базы знаний (*.kdb)|*.kdb|Все файлы (*.*)|*.*";

            if (_openFileDialog.ShowDialog() != true) return;

            lock (_lockObj)
            {
                ReadFile();
            }

            RaisePropertyChanged(nameof(ForQuestionList));
            RaisePropertyChanged(nameof(ForResultList));
        }

        public ICommand ResultCommand => new RelayCommand(ResultCommandMethod);

        private void ResultCommandMethod()
        {
            if (_currentFact == null)return;
            Items.Clear();
            GetWorkResult();
        }

        

        private void ReadFile()
        {
            using var stream = new FileStream(_openFileDialog.FileName, FileMode.Open);
            using var reader = new BinaryReader(stream);

            if (!IsValidHeaderDataBase(reader)) return;

            _rules.Clear();
            _questions.Clear();
            _facts.Clear();


            var factCount = reader.ReadInt32();

            for (var i = 0; i < factCount; i++)
            {
                _facts.Add(new Fact().ReadFromFile(reader));
            }

            var ruleCount = reader.ReadInt32();

            for (var i = 0; i < ruleCount; i++)
            {
                _rules.Add(new Rule().ReadFromFile(reader));
            }

            var questionCount = reader.ReadInt32();

            for (var i = 0; i < questionCount; i++)
            {
                _questions.Add(new Question().ReadFromFile(reader));
            }
        }

        private bool IsValidHeaderDataBase(BinaryReader reader)
        {
            var fileHeader = new DataBaseMagicNumber {Magic = reader.ReadInt32(), Version = reader.ReadInt32()};

            return fileHeader.Magic == 0x773E975E || fileHeader.Version == 0x10000000;
        }

        public Rule CurrentRule
        {
            get => _currentRule;
            set => SetProperty(ref _currentRule, value);
        }

        public Fact CurrentFact
        {
            get => _currentFact;
            set => SetProperty(ref _currentFact, value);
        }

        public Question CurrentQuestion
        {
            get => _currentQuestion;
            set => SetProperty(ref _currentQuestion, value);
        }

        private bool IsValidFact(string sFact)
        {
            bool found = false;

            // Проверяет существование факта sFact
            foreach (Fact fact in _facts)
            {
                if (fact.ToString() == sFact)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }


        public void GetWorkResult()
        {
            _trueFactsList = new ArrayList();
            foreach (var fact1 in _facts.Where(fact => fact.IsSelected))
            {
                _trueFactsList.Add(fact1);
            }

            var work_truths = _facts.Where(fact => fact.IsSelected).Select(fact => fact.Truth).ToArray();

            
              _header = "Проверяем - " + _currentFact;

            ArrayList temp_facts = new ArrayList();
            ArrayList facts_to_check = new ArrayList();
            ArrayList temp_facts_to_check = new ArrayList();
            ArrayList temp_rules = new ArrayList();
            ArrayList temp_converted_rules = new ArrayList();


            facts_to_check.Add(_currentFact.ToString());

            int iteration_num = 0;
            bool was_new_facts = true;

            /////////////////////////////
            /////////////////////////////


            while (was_new_facts)
            {
                var iteration = new FirstItem()
                {
                    Name = ("Итерация " + (iteration_num + 1).ToString())
                };

                /////////////////////////////
                /////////////////////////////
                /////////////////////////////

                var factIsValid = new SecontItem();
                var ruleNoValid = new SecontItem();
                var conflictManu = new SecontItem();
                var ruleItem = new ThreeItem();
                var tnRule = new SecontItem();

                var factMemory = new SecontItem()
                {
                    Name = ("Факты в памяти")
                };

                for (int f_num = 0; f_num < _trueFactsList.Count; f_num++)
                {
                    factMemory.TreeViews.Add(new ThreeItem()
                    {
                        Name = (_trueFactsList[f_num]).ToString() + " : " + ((double) work_truths[f_num]).ToString()
                    });
                }

         
                /////////////////////////////
                /////////////////////////////
                /////////////////////////////


                if (facts_to_check.Count > 0)
                {
                    factIsValid = new SecontItem()
                    {
                        Name = ("Факты для проверки")
                    };


                    foreach (string sFact in facts_to_check)
                    {
                        factIsValid.TreeViews.Add(new ThreeItem()
                        {
                            Name = sFact
                        });
                    }
                }

                /////////////////////////////
                /////////////////////////////
                /////////////////////////////

                if (_trueFactsList.Contains(_currentFact.ToString()))
                    break;

                temp_facts.Clear();
                temp_facts_to_check.Clear();
                temp_rules.Clear();
                temp_converted_rules.Clear();
                was_new_facts = false;

                foreach (string sFact in facts_to_check)
                {
                    foreach (Rule rule in _rules)
                    {
                        bool found = false;
                        int fact_num = -1;

                        for (int f_num = 0; f_num < rule.Conclusions.Count; f_num++)
                        {
                            int fact_id = (int) rule.Conclusions[f_num];

                            // Находим факт с таким ID
                            foreach (Fact fact in _facts)
                            {
                                if (fact.ID == fact_id &&
                                    fact.ToString() == sFact)
                                {
                                    found = true;
                                    fact_num = f_num;

                                    break;
                                }
                            }
                        }

                        if (!found) continue;

                        // Проверяем истинность посылки факта
                        ArrayList condition_facts = new ArrayList();
                        string converted_rule = "";
                        string s = "";
                        int pos = 0;

                        while (pos < rule.Condition.Length)
                        {
                            char sc = rule.Condition[pos];

                            if (sc == '(' || sc == ')' || sc == '&' || sc == '|')
                            {
                                if (s != "")
                                {
                                    s = s.Trim();
                                    converted_rule += (_trueFactsList.Contains(s) ? '1' : '0').ToString();
                                    if (IsValidFact(s))
                                        condition_facts.Add(s);
                                    s = "";
                                }

                                converted_rule += sc;
                            }
                            else
                                s += sc; // todo;

                            pos++;
                        }

                        if (s != "")
                        {
                            s = s.Trim();
                            converted_rule += (_trueFactsList.Contains(s) ? '1' : '0');
                            if (IsValidFact(s))
                                condition_facts.Add(s);
                        }

                        if (CheckExpression(converted_rule)) // Rule is true
                        {
                            // Проверяем, есть ли все факты из заключения уже в рабочей
                            // памяти
                            bool all_conclusion_facts_is_in_work_facts = true;

                            for (int f_num = 0; f_num < rule.Conclusions.Count; f_num++)
                            {
                                int fact_id = (int) rule.Conclusions[f_num];

                                // Находим факт с таким ID
                                Fact fact = GetFactByID(fact_id);
                                if (fact != null)
                                {
                                    if (!_trueFactsList.Contains(fact.ToString()))
                                    {
                                        all_conclusion_facts_is_in_work_facts = false;
                                        break;
                                    }
                                }
                            }

                            if (!all_conclusion_facts_is_in_work_facts)
                            {
                                // Хотя бы один факт из заключения правила отсутствует в рабочей памяти
                                temp_rules.Add(rule);
                                temp_converted_rules.Add(converted_rule);
                            }
                        }
                        
                        else
                        {
                            // Если правило не сработало, то добавляем факты из посылки
                            // правила в список фактов для проверки
                            var isFirst = true;
                            foreach (string sConditionFact in condition_facts)
                            {
                                if (!facts_to_check.Contains(sConditionFact) &&
                                    !temp_facts_to_check.Contains(sConditionFact) &&
                                    !_trueFactsList.Contains(sConditionFact))
                                {
                                    /////////////////////////////
                                    /////////////////////////////
                                    /////////////////////////////
                                    //Todo:2

                                    if (isFirst)
                                    {
                                        ruleNoValid = new SecontItem()
                                        {
                                            Name = "Правило \"" + rule.Name +
                                                   "\" не сработало, но из посылки следующие факты были добавлены в список фактов для проверки:"
                                        };

                                        isFirst = !isFirst;
                                    }

                                    temp_facts_to_check.Add(sConditionFact);

                                    ruleNoValid.TreeViews.Add(new ThreeItem()
                                    {
                                        Name = sConditionFact
                                    });

                                    was_new_facts = true;
                                }
                            }
                        }
                    }
                }

                /////////////////////////////
                /////////////////////////////
                /////////////////////////////


                // Добавляем накопившиеся факты для проверки в список фактов для проверки
                foreach (string sFact in temp_facts_to_check)
                    if (!facts_to_check.Contains(sFact))
                        facts_to_check.Add(sFact);

                Rule active_rule = (temp_rules.Count > 0) ? (Rule)temp_rules[0] : null;
                string active_converted_rule = (temp_converted_rules.Count > 0) ? (string)temp_converted_rules[0] : null;

                if (temp_rules.Count > 1)
                {
                    /////////////////////////////
                    /////////////////////////////
                    /////////////////////////////
                    conflictManu = new SecontItem()
                    {
                        Name = "Конфликтное множество"
                    };

                    //tnIteration.Nodes.Add(tnConflictSet);
                   

                    for (int rule_num = 0; rule_num < temp_rules.Count; rule_num++)
                    {
                        Rule rule = (Rule)temp_rules[rule_num];

                        ruleItem = new ThreeItem()
                        {
                            Name = rule.Name + " : " + rule.Truth.ToString() + " [" +
                                   (string)temp_converted_rules[rule_num] + "]"
                        };

                        string tnCondition = rule.Condition;
                      
                        //tnRule.Nodes.Add(tnCondition);

                        for (int f_num = 0; f_num < rule.Conclusions.Count; f_num++)
                        {
                            int fact_id = (int)rule.Conclusions[f_num];

                            // Находим факт с таким ID
                            Fact fact = GetFactByID(fact_id);
                            if (fact != null)
                            {
                                string tnConclusion = fact.ToString();

                                ruleItem.TreeViews.Add(new FourItem()
                                {
                                    Name = tnConclusion
                                });
                                //tnRule.Nodes.Add(tnConclusion);
                            }
                        }
                        conflictManu.TreeViews.Add(ruleItem);


                        switch (0)
                        {
                            case 0:
                                // Set max rule truth for current fact
                                if (rule.Truth > active_rule.Truth)
                                {
                                    active_rule = rule;
                                    active_converted_rule = (string)temp_converted_rules[rule_num];
                                }
                                break;
                            case 1:
                                // Last rule
                                active_rule = rule;
                                active_converted_rule = (string)temp_converted_rules[rule_num];
                                break;
                            case 2:
                                // Rule with maximum facts in the condition

                                // Count operators in the rule condition
                                int r_op_cnt = 0, ar_op_cnt = 0;
                                int c_pos = 0;

                                while (c_pos < rule.Condition.Length)
                                {
                                    if (rule.Condition[c_pos] == '|' || rule.Condition[c_pos] == '&')
                                        r_op_cnt++;
                                    c_pos++;
                                }

                                // Count operators in the active rule condition
                                c_pos = 0;
                                while (c_pos < rule.Condition.Length)
                                {
                                    if (rule.Condition[c_pos] == '|' || rule.Condition[c_pos] == '&')
                                        ar_op_cnt++;
                                    c_pos++;
                                }

                                if (r_op_cnt > ar_op_cnt)
                                {
                                    active_rule = rule;
                                    active_converted_rule = (string)temp_converted_rules[rule_num];
                                }
                                break;
                            case 3: break; // Do not touch first rule
                        }
                    }
                }

                if (active_rule != null)
                {
                    //Todo:AddIteration
                    tnRule = new SecontItem()
                    {
                        Name = "Сработало правило: " + active_rule.Name + " : " + active_rule.Truth.ToString() + " [" + active_converted_rule + "]"
                    };
                    //tnIteration.Nodes.Add(tnRule);

                    tnRule.TreeViews.Add(new ThreeItem()
                    {
                        Name = active_rule.Condition
                    });
                    
                    //tnRule.Nodes.Add(tnCondition);

                    for (int f_num = 0; f_num < active_rule.Conclusions.Count; f_num++)
                    {
                        int fact_id = (int)active_rule.Conclusions[f_num];

                        // Находим факт с таким ID
                        Fact fact = GetFactByID(fact_id);
                        if (fact != null)
                        {
                            string tnConclusion = fact.ToString();
                            tnRule.TreeViews.Add(new ThreeItem()
                            {
                                Name = tnConclusion
                            });
                          
                            //tnRule.Nodes.Add(tnConclusion);
                        }
                    }

                    foreach (int fact_id in active_rule.Conclusions)
                    {
                        // Находим факт с таким ID
                        Fact fact = GetFactByID(fact_id);
                        if (fact != null)
                        {
                            Fact new_fact = new Fact()
                            {
                                ID = -1,
                                Truth = active_rule.Truth,
                                IsSelected = default,
                                Value = fact.Value,
                                Type = fact.Type,
                                Attribute = fact.Attribute,
                                Object = fact.Object
                            };

                            int index = temp_facts.IndexOf(new_fact);
                            if (index == -1)
                            {
                                temp_facts.Add(new_fact);
                            }
                        }
                    }
                }

                // Анализируем собранные факты
                foreach (Fact fact in temp_facts)
                {
                    // Add this fact to the work facts list
                    int index = _trueFactsList.IndexOf(fact.ToString());
                    if (index == -1)
                    {
                        _trueFactsList.Add(fact.ToString());
                        ((IList) work_truths).Add(fact.Truth);
                        was_new_facts = true;

                        // Удаляем этот факт из списка фактов для проверки
                        facts_to_check.Remove(fact.ToString());
                    }
                    else
                    {
                        switch (1)
                        {
                            case 0:
                                // Set min truth for current fact
                                if (fact.Truth < (double)work_truths[index])
                                {
                                    work_truths[index] = fact.Truth;
                                    //was_new_facts = true;
                                }
                                break;
                            case 1:
                                work_truths[index] = fact.Truth;
                                //was_new_facts = true;
                                break;
                            case 2: break; // It is not new fact so do anything
                        }
                    }
                }


                iteration.TreeViews.Add(factMemory);
                iteration.TreeViews.Add(factIsValid);
                iteration.TreeViews.Add(ruleNoValid);
                iteration.TreeViews.Add(conflictManu);
                iteration.TreeViews.Add(ruleItem);
                iteration.TreeViews.Add(tnRule);

                Items.Add(iteration);
                iteration_num++;
            }
        }

        private Fact GetFactByID(int fact_id)
        {
            foreach (Fact fact in _facts)
                if (fact.ID == fact_id)
                    return fact;

            return null;
        }

        private bool CheckExpression(string exp)
        {
            Stack st = new Stack();

            string s = "";
            char sc = ' ';
            int pos = 0;

            while (pos < exp.Length)
            {
                char c = exp[pos];

                if (c == ')')
                {
                    s = "";
                    while (st.Count > 0 && (sc = (char)st.Pop()) != '(') s += sc;

                    if (sc != '(') return false; // Error

                    st.Push(CheckExpression(s) ? '1' : '0');
                }
                else
                    st.Push(c);

                pos++;
            }

            s = "";
            while (st.Count > 0) s += (char)st.Pop();

            string token1 = "", token2 = "";

            pos = 0;

            while (pos < s.Length)
            {
                sc = s[pos];

                if (sc == '|' || sc == '&')
                    break;
                token1 += s[pos];

                pos++;
            }

            token1 = token1.Trim();

            if (pos == s.Length)
            {
                // One token
                return (token1 == "1") ? true : false;
            }

            char op = sc;
            pos++;

            while (pos < s.Length)
            {
                sc = s[pos];

                if (sc == '|' || sc == '&')
                {
                    op = sc;
                    switch (sc)
                    {
                        case '|': token1 = ((token1 == "0" && token2 == "0") ? "0" : "1"); break;
                        case '&': token1 = ((token1 == "1" && token2 == "1") ? "1" : "0"); break;
                    }
                    //return false; // Error! Allowed only "token1" or "token1&token2"
                    token2 = "";
                }
                else
                    token2 += sc;

                pos++;
            }

            token2 = token2.Trim();

            switch (op)
            {
                case '|': return (token1 == "0" && token2 == "0") ? false : true;
                case '&': return (token1 == "1" && token2 == "1") ? true : false;
            }

            return false;
        }
    }
}

