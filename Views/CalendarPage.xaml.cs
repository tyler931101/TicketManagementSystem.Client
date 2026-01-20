using System.Windows.Controls;
using System.Windows;
using System;
using System.Windows.Media;
using System.Collections.Generic;

namespace TicketManagementSystem.Client.Views
{
    public partial class CalendarPage : Page
    {
        private DateTime _currentDate;
        private int _currentYear;
        private string _currentViewMode = "Month";

        public CalendarPage()
        {
            InitializeComponent();
            
            // Initialize with current date
            _currentDate = DateTime.Now;
            _currentYear = DateTime.Now.Year;
            
            // Set up event handlers
            PreviousMonthBtn.Click += PreviousMonthBtn_Click;
            NextMonthBtn.Click += NextMonthBtn_Click;
            ViewComboBox.SelectionChanged += ViewComboBox_SelectionChanged;
            TodayButton.Click += TodayButton_Click;
            ShowTicketsCheckBox.Checked += CheckBox_CheckedChanged;
            ShowTicketsCheckBox.Unchecked += CheckBox_CheckedChanged;
            ShowTasksCheckBox.Checked += CheckBox_CheckedChanged;
            ShowTasksCheckBox.Unchecked += CheckBox_CheckedChanged;
            
            // Update calendar display
            UpdateCalendarDisplay();
        }

        private void CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            UpdateCalendarDisplay();
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = DateTime.Now;
            _currentYear = DateTime.Now.Year;
            UpdateCalendarDisplay();
        }

        private void ViewComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content != null)
            {
                _currentViewMode = selectedItem.Content.ToString();
                UpdateCalendarDisplay();
            }
        }

        private void PreviousMonthBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentViewMode)
            {
                case "Day":
                    _currentDate = _currentDate.AddDays(-1);
                    break;
                case "Week":
                    _currentDate = _currentDate.AddDays(-7);
                    break;
                case "Month":
                    _currentDate = _currentDate.AddMonths(-1);
                    break;
                case "Year":
                    _currentYear--;
                    break;
            }
            UpdateCalendarDisplay();
        }

        private void NextMonthBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (_currentViewMode)
            {
                case "Day":
                    _currentDate = _currentDate.AddDays(1);
                    break;
                case "Week":
                    _currentDate = _currentDate.AddDays(7);
                    break;
                case "Month":
                    _currentDate = _currentDate.AddMonths(1);
                    break;
                case "Year":
                    _currentYear++;
                    break;
            }
            UpdateCalendarDisplay();
        }

        private void UpdateCalendarDisplay()
        {
            // Update month/year display based on view mode
            switch (_currentViewMode)
            {
                case "Day":
                    CurrentMonthText.Text = $"{_currentDate:dddd, MMMM d, yyyy}";
                    break;
                case "Week":
                    DateTime weekStart = GetWeekStart(_currentDate);
                    DateTime weekEnd = weekStart.AddDays(6);
                    CurrentMonthText.Text = $"{weekStart:MMM d} - {weekEnd:MMM d, yyyy}";
                    break;
                case "Month":
                    CurrentMonthText.Text = $"{_currentDate:MMMM yyyy}";
                    break;
                case "Year":
                    CurrentMonthText.Text = $"{_currentYear}";
                    break;
            }
            
            // Clear existing calendar days
            CalendarDaysGrid.Children.Clear();
            CalendarDaysGrid.RowDefinitions.Clear();
            CalendarDaysGrid.ColumnDefinitions.Clear();
            
            switch (_currentViewMode)
            {
                case "Day":
                    SetupDayView();
                    break;
                case "Week":
                    SetupWeekView();
                    break;
                case "Month":
                    SetupMonthView();
                    break;
                case "Year":
                    SetupYearView();
                    break;
            }
        }

        private DateTime GetWeekStart(DateTime date)
        {
            DateTime start = date;
            while (start.DayOfWeek != DayOfWeek.Sunday)
            {
                start = start.AddDays(-1);
            }
            return start;
        }

        private void SetupDayView()
        {
            // Single cell for day view
            CalendarDaysGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            CalendarDaysGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            
            var dayCell = CreateDayCell(_currentDate, isCurrentMonth: true);
            Grid.SetRow(dayCell, 0);
            Grid.SetColumn(dayCell, 0);
            CalendarDaysGrid.Children.Add(dayCell);
        }

        private void SetupWeekView()
        {
            // 1 row, 7 columns for week view
            CalendarDaysGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < 7; i++)
            {
                CalendarDaysGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            
            DateTime weekStart = GetWeekStart(_currentDate);
            
            for (int i = 0; i < 7; i++)
            {
                DateTime currentDay = weekStart.AddDays(i);
                bool isCurrentMonth = currentDay.Month == _currentDate.Month;
                var dayCell = CreateDayCell(currentDay, isCurrentMonth);
                Grid.SetRow(dayCell, 0);
                Grid.SetColumn(dayCell, i);
                CalendarDaysGrid.Children.Add(dayCell);
            }
        }

        private void SetupMonthView()
        {
            // Get the first day of the month
            DateTime firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            
            // Get the first day to display (may be from previous month)
            DateTime displayStartDate = GetFirstDisplayDate(firstDayOfMonth);
            
            // Get the last day to display (may be from next month)
            DateTime displayEndDate = GetLastDisplayDate(firstDayOfMonth);
            
            // Calculate number of weeks needed
            int totalDays = (int)(displayEndDate - displayStartDate).TotalDays + 1;
            int weeksNeeded = totalDays / 7;
            
            // Create rows based on weeks needed (always 6 to show full weeks)
            for (int i = 0; i < weeksNeeded; i++)
            {
                CalendarDaysGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            
            // 7 columns for days of week
            for (int i = 0; i < 7; i++)
            {
                CalendarDaysGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            
            // Add all days for the displayed period
            DateTime currentDate = displayStartDate;
            int currentRow = 0;
            int currentColumn = 0;
            
            while (currentDate <= displayEndDate)
            {
                bool isCurrentMonth = currentDate.Month == _currentDate.Month;
                var dayCell = CreateDayCell(currentDate, isCurrentMonth);
                Grid.SetRow(dayCell, currentRow);
                Grid.SetColumn(dayCell, currentColumn);
                CalendarDaysGrid.Children.Add(dayCell);
                
                currentColumn++;
                if (currentColumn >= 7)
                {
                    currentColumn = 0;
                    currentRow++;
                }
                currentDate = currentDate.AddDays(1);
            }
        }

        private DateTime GetFirstDisplayDate(DateTime firstDayOfMonth)
        {
            // Start from the first day of the month and go back to Sunday
            DateTime displayStart = firstDayOfMonth;
            while (displayStart.DayOfWeek != DayOfWeek.Sunday)
            {
                displayStart = displayStart.AddDays(-1);
            }
            return displayStart;
        }

        private DateTime GetLastDisplayDate(DateTime firstDayOfMonth)
        {
            // Get the last day of the month
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            // Go forward to Saturday
            DateTime displayEnd = lastDayOfMonth;
            while (displayEnd.DayOfWeek != DayOfWeek.Saturday)
            {
                displayEnd = displayEnd.AddDays(1);
            }
            return displayEnd;
        }

        private void SetupYearView()
        {
            // 3 rows, 4 columns for year view
            for (int i = 0; i < 3; i++)
            {
                CalendarDaysGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < 4; i++)
            {
                CalendarDaysGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            
            // Display 12 months in a 4x3 grid
            int month = 1;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    if (month <= 12)
                    {
                        var monthCell = CreateMonthCell(month);
                        Grid.SetRow(monthCell, row);
                        Grid.SetColumn(monthCell, col);
                        CalendarDaysGrid.Children.Add(monthCell);
                        month++;
                    }
                }
            }
        }

        private Border CreateMonthCell(int month)
        {
            DateTime firstDayOfMonth = new DateTime(_currentYear, month, 1);
            string monthName = firstDayOfMonth.ToString("MMM");
            bool isCurrentMonth = month == DateTime.Now.Month && _currentYear == DateTime.Now.Year;
            
            var textBlock = new TextBlock
            {
                Text = monthName,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                FontWeight = isCurrentMonth ? FontWeights.Bold : FontWeights.Normal,
                Foreground = isCurrentMonth 
                    ? new SolidColorBrush(Color.FromRgb(31, 41, 55))
                    : new SolidColorBrush(Color.FromRgb(107, 114, 128))
            };

            var border = new Border
            {
                Background = isCurrentMonth 
                    ? new SolidColorBrush(Color.FromRgb(254, 243, 199))
                    : Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(20),
                Child = textBlock,
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = month
            };

            // Add hover effect
            border.MouseEnter += (s, e) =>
            {
                if (!isCurrentMonth)
                {
                    border.Background = new SolidColorBrush(Color.FromRgb(243, 244, 246));
                }
            };

            border.MouseLeave += (s, e) =>
            {
                if (!isCurrentMonth)
                {
                    border.Background = Brushes.White;
                }
            };

            border.MouseLeftButtonDown += (s, e) =>
            {
                // Switch to month view when month is clicked
                _currentDate = new DateTime(_currentYear, month, 1);
                _currentViewMode = "Month";
                ViewComboBox.SelectedIndex = 0; // Set to Month view
                UpdateCalendarDisplay();
            };

            return border;
        }

        private Border CreateDayCell(DateTime date, bool isCurrentMonth = true)
        {
            bool isToday = date.Date == DateTime.Now.Date;
            bool isWeekend = date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday;
            
            var dayText = new TextBlock
            {
                Text = date.Day.ToString(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                FontSize = 14,
                FontWeight = isToday ? FontWeights.Bold : FontWeights.Normal,
                Foreground = isCurrentMonth 
                    ? new SolidColorBrush(Color.FromRgb(31, 41, 55))
                    : new SolidColorBrush(Color.FromRgb(156, 163, 175))
            };

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5)
            };
            stackPanel.Children.Add(dayText);

            // Add sample events (only for current month days)
            if (isCurrentMonth)
            {
                if (ShowTicketsCheckBox.IsChecked == true && date.Day % 5 == 0)
                {
                    var ticketEvent = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                        CornerRadius = new CornerRadius(3),
                        Margin = new Thickness(0, 2, 0, 0),
                        Padding = new Thickness(5, 2, 5, 2)
                    };
                    
                    var ticketText = new TextBlock
                    {
                        Text = "Ticket #" + date.Day,
                        FontSize = 10,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    
                    ticketEvent.Child = ticketText;
                    stackPanel.Children.Add(ticketEvent);
                }

                if (ShowTasksCheckBox.IsChecked == true && date.Day % 3 == 0)
                {
                    var taskEvent = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                        CornerRadius = new CornerRadius(3),
                        Margin = new Thickness(0, 2, 0, 0),
                        Padding = new Thickness(5, 2, 5, 2)
                    };
                    
                    var taskText = new TextBlock
                    {
                        Text = "Task " + date.Day,
                        FontSize = 10,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    
                    taskEvent.Child = taskText;
                    stackPanel.Children.Add(taskEvent);
                }
            }

            var border = new Border
            {
                Background = isToday 
                    ? new SolidColorBrush(Color.FromRgb(254, 243, 199))
                    : (isCurrentMonth ? Brushes.White : new SolidColorBrush(Color.FromRgb(249, 250, 251))),
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235)),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Padding = new Thickness(5),
                Child = stackPanel,
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = date
            };

            // Add hover effect
            border.MouseEnter += (s, e) =>
            {
                if (!isToday)
                {
                    border.Background = new SolidColorBrush(Color.FromRgb(243, 244, 246));
                }
            };

            border.MouseLeave += (s, e) =>
            {
                if (!isToday)
                {
                    border.Background = isCurrentMonth ? Brushes.White : new SolidColorBrush(Color.FromRgb(249, 250, 251));
                }
            };

            border.MouseLeftButtonDown += (s, e) =>
            {
                // If clicking on a day from another month, navigate to that month
                if (!isCurrentMonth && _currentViewMode == "Month")
                {
                    _currentDate = new DateTime(date.Year, date.Month, 1);
                    UpdateCalendarDisplay();
                }
                else
                {
                    // Handle day selection
                    MessageBox.Show($"Selected date: {date:yyyy-MM-dd}", "Date Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            return border;
        }
    }
}