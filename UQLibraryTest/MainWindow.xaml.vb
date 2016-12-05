Imports OpenQA.Selenium.Chrome, OpenQA.Selenium.Support.UI, OpenQA.Selenium

Imports Newtonsoft.Json
Imports System.Text.RegularExpressions
Imports System.ComponentModel

Class MainWindow

    Public TestLogs As New System.Collections.ObjectModel.ObservableCollection(Of TestLog)


    Public Libraries As Library()
    Public Computers As ComputerAavailability()
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        gdLog.ItemsSource = TestLogs
        Chrome = New ChromeDriver

        'download back end data

        Dim wc As New System.Net.WebClient
        Dim setting = New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore, .MissingMemberHandling = MissingMemberHandling.Ignore}
        Dim hours = wc.DownloadString("http://app.library.uq.edu.au/api/v2/library_hours/day")
        Libraries = JsonConvert.DeserializeObject(Of HoursObject)(hours, setting).locations
        Dim coms = wc.DownloadString("https://app.library.uq.edu.au/api/computer_availability")
        Computers = JsonConvert.DeserializeObject(Of ComputerAavailability())(coms, setting)
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        'close both the browser and chrome driver at the end of test
        Chrome.Quit()
    End Sub

    Public Chrome As ChromeDriver

    Public Sub Log(Action As String, Optional Comment As String = "", Optional Result As String = "", Optional Failed As String = "", Optional ActionBrush As Brush = Nothing)
        If ActionBrush Is Nothing Then ActionBrush = Brushes.White
        Dim log = New TestLog With {.Action = Action, .Comment = Comment, .Result = Result, .Failed = Failed, .ActionBrush = ActionBrush}
        TestLogs.Add(log)
    End Sub

    Public Sub WaitUntil(Condition As Func(Of IWebDriver, IWebElement), Optional ErrorMessage As String = "Wait Timeout", Optional Seconds As Integer = 10)
        Try
            Dim wait = New WebDriverWait(Chrome, New TimeSpan(0, 0, Seconds))
            wait.Until(Condition)
        Catch ex As Exception
            Log(ErrorMessage)
        End Try
    End Sub
    Public Sub BeginTest(Optional Name As String = "")
        Log("--- Begin", Name,,, Brushes.Red)
    End Sub
    Public Sub EndTest(Optional Name As String = "")
        Log("--- End", Name,,, Brushes.Blue)
        Log("")
    End Sub
    Public Sub ResetURL()
        Chrome.Url = "about:blank"
        Chrome.Navigate()
    End Sub

    Private Sub ViewLibraries(sender As Object, e As RoutedEventArgs)
        BeginTest("View Library List")
        ResetURL()
        Log("View library lists", "http://localhost")
        Chrome.Url = "http://localhost"
        Chrome.Navigate()

        Log("wait until the today element is visible")
        WaitUntil(ExpectedConditions.ElementIsVisible(By.TagName("today")))
        Log("Number of Today", "Should be 1", Chrome.FindElementsByTagName("today").Count)

        Log("wait until the div.lib-title are visible")
        WaitUntil(ExpectedConditions.ElementIsVisible(By.CssSelector("div.lib-title")))
        Log("Number of Library Title", "Should be " + Libraries.Length.ToString, Chrome.FindElementsByCssSelector("div.lib-title").Count)

        EndTest("View Library List")
        'count the number of libraries
    End Sub

    Private Sub ViewLibraryHours(sender As Object, e As RoutedEventArgs)
        BeginTest("View List of Library Hours")
        ResetURL()
        Log("View library lists", "http://localhost/#/week")
        Chrome.Url = "http://localhost/#/week"
        Chrome.Navigate()

        Log("wait until the week element is visible")
        WaitUntil(ExpectedConditions.ElementIsVisible(By.TagName("week")))
        Log("Number of week", "Should be 1", Chrome.FindElementsByTagName("week").Count)

        Log("wait until the div.lib-title are visible")
        WaitUntil(ExpectedConditions.ElementIsVisible(By.CssSelector("div.lib-title")))
        Log("Number of Library Title", "Should be" + Libraries.Length.ToString, Chrome.FindElementsByCssSelector("div.lib-title").Count)
        EndTest("View Library Hours")
    End Sub

    Private Sub FilterWithKeyword(sender As Object, e As RoutedEventArgs)
        'keyword-filter
        BeginTest("Filter libraries with name")
        ResetURL()
        Log("View library lists", "http://localhost/#/today")
        Chrome.Url = "http://localhost/#/today"
        Chrome.Navigate()

        Log("wait until the today element is visible")
        WaitUntil(ExpectedConditions.ElementIsVisible(By.TagName("today")))
        Log("Number of week", "Should be 1", Chrome.FindElementsByTagName("today").Count)

        Log("wait until the .keyword-filter is visible")
        WaitUntil(ExpectedConditions.ElementIsVisible(By.CssSelector(".keyword-filter")))
        Log("Number of week", "Should be 1", Chrome.FindElementsByCssSelector(".keyword-filter").Count)
        Randomize()
        Dim key = Chr(CInt(Asc("A"c)) + Math.Round(25 * Rnd()))
        If Chrome.FindElementsByCssSelector(".keyword-filter").Count = 1 Then
            Dim element = Chrome.FindElementByCssSelector(".keyword-filter")
            element.SendKeys(key)
        End If

        Log("Number of Library Title", "Should be " + (Libraries.Count(Function(item As Library) Regex.IsMatch(item.name, key, RegexOptions.IgnoreCase))).ToString, Chrome.FindElementsByCssSelector("div.lib-title").Count)

        EndTest("Filter libraries with name")
    End Sub
    ''' <summary>
    ''' Randomly select a library from the data set and access its page for detailed hours
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ViewLibraryOpenningHours(sender As Object, e As RoutedEventArgs)
        BeginTest("View Library Hours")
        ResetURL()
        Randomize()
        Dim id = Libraries(Math.Floor(Libraries.Length * Rnd())).lid
        Log("View library by id", "http://localhost/#/" + id.ToString)
        Chrome.Url = "http://localhost/#/" + id.ToString
        Chrome.Navigate()

        Log("wait until the detail element is visible")
        WaitUntil(ExpectedConditions.ElementIsVisible(By.TagName("detail")))
        Log("Number of detail", "Should be 1", Chrome.FindElementsByTagName("detail").Count)

        'rendered-hours
        Log("wait until the .rendered-hours is visible")
        WaitUntil(ExpectedConditions.ElementExists(By.CssSelector(".rendered-hours")))
        Log("Number of rendered-hours", , Chrome.FindElementsByCssSelector(".rendered-hours").Count)

        EndTest("View Library Hours")
    End Sub

    Private Sub AccessLibraryByID(sender As Object, e As RoutedEventArgs)
        BeginTest("View Library Hours")
        ResetURL()
        Randomize()
        Dim id = Libraries(Math.Floor(Libraries.Length * Rnd())).lid
        Log("View library by id", "http://localhost/#/" + id.ToString)
        Chrome.Url = "http://localhost/#/" + id.ToString
        Chrome.Navigate()

        Log("wait until the detail element is visible")
        WaitUntil(ExpectedConditions.ElementIsVisible(By.TagName("detail")))
        Log("Number of detail", "Should be 1", Chrome.FindElementsByTagName("detail").Count)

        'rendered-hours
        Log("wait until the .lib-id is visible")
        WaitUntil(ExpectedConditions.ElementExists(By.CssSelector(".lib-id")))
        Log("Number of lib-id", "Should be 1", Chrome.FindElementsByCssSelector(".lib-id").Count)
        If Chrome.FindElementsByCssSelector(".lib-id").Count = 1 Then
            Log("Value lib-id", "Should be " + id.ToString, Chrome.FindElementByCssSelector(".lib-id").Text)
        End If


        EndTest("View Library Hours")
    End Sub

    Private Sub ViewAvailableComputers(sender As Object, e As RoutedEventArgs)
        'room-computers all-computers
        BeginTest("View Available Computers")
        ResetURL()
        Randomize()
        Dim id = Libraries(Math.Floor(Libraries.Length * Rnd())).lid
        Log("View library by id", "http://localhost/#/" + id.ToString)
        Chrome.Url = "http://localhost/#/" + id.ToString
        Chrome.Navigate()

        Log("wait until the detail element is visible")
        WaitUntil(ExpectedConditions.ElementIsVisible(By.TagName("detail")))
        Log("Number of detail", "Should be 1", Chrome.FindElementsByTagName("detail").Count)

        'rendered-hours
        Log("wait until the .all-computers is visible")
        Log("Number of all-computers", "Should be 1 if this library has computers", Chrome.FindElementsByCssSelector(".all-computers").Count)
        If Chrome.FindElementsByCssSelector(".all-computers").Count = 1 Then
            Log("Value all-computers", , Chrome.FindElementByCssSelector(".all-computers").Text)
        End If

        Log("wait until the .room-computers is visible")
        Log("Number of room-computers", "Should be >0 if this library has computer rooms", Chrome.FindElementsByCssSelector(".room-computers").Count)
        Log("Computer info of each room")
        For Each el In Chrome.FindElementsByCssSelector(".room-computers")
            Log("Value of room-computers", , el.Text)
        Next

        EndTest("View Available Computers")
    End Sub

    Private Sub CreateNewLibraryWithInvalidValue(sender As Object, e As RoutedEventArgs)
        BeginTest("Create New Library With Invalid Values")
        ResetURL()

        Dim url = "http://localhost/#/new"

        Log("Navigate to URL: ", url)
        Chrome.Url = url
        Chrome.Navigate()

        Log("wait until the newlib element is visible")
        WaitUntil(ExpectedConditions.ElementIsVisible(By.TagName("newlib")))
        Log("Number of newlib", "Should be 1", Chrome.FindElementsByTagName("newlib").Count)

        Dim allfound As Boolean = True


        Dim elID = Chrome.FindElementByCssSelector("input[name=""id""]")
        Log("Input for id",, IIf(elID IsNot Nothing, "Yes", "No"))
        If elID Is Nothing Then
            allfound = False
        End If

        Dim elAbbr = Chrome.FindElementByCssSelector("input[name=""abbr""]")
        Log("Input for abbr",, IIf(elAbbr IsNot Nothing, "Yes", "No"))
        If elAbbr Is Nothing Then
            allfound = False
        End If

        Dim elName = Chrome.FindElementByCssSelector("input[name=""name""]")
        Log("Input for name",, IIf(elName IsNot Nothing, "Yes", "No"))
        If elName Is Nothing Then
            allfound = False
        End If

        Dim elCampus = Chrome.FindElementByCssSelector("md-select[name=""campus""]")
        Log("Input for campus",, IIf(elCampus IsNot Nothing, "Yes", "No"))
        If elCampus Is Nothing Then
            allfound = False
        End If

        Dim elDesc = Chrome.FindElementByCssSelector("input[name=""desc""]")
        Log("Input for desc",, IIf(elDesc IsNot Nothing, "Yes", "No"))
        If elDesc Is Nothing Then
            allfound = False
        End If

        Dim elURL = Chrome.FindElementByCssSelector("input[name=""url""]")
        Log("Input for url",, IIf(elURL IsNot Nothing, "Yes", "No"))
        If elURL Is Nothing Then
            allfound = False
        End If

        Dim elLat = Chrome.FindElementByCssSelector("input[name=""lat""]")
        Log("Input for latitude",, IIf(elLat IsNot Nothing, "Yes", "No"))
        If elLat Is Nothing Then
            allfound = False
        End If

        Dim elLong = Chrome.FindElementByCssSelector("input[name=""long""]")
        Log("Input for longitude",, IIf(elLong IsNot Nothing, "Yes", "No"))
        If elLong Is Nothing Then
            allfound = False
        End If

        Dim elBtnCancel = Chrome.FindElementByCssSelector(".create-cancel")
        Log("Cancel Button",, IIf(elBtnCancel IsNot Nothing, "Yes", "No"))
        If elBtnCancel Is Nothing Then
            allfound = False
        End If

        Dim elBtnSave = Chrome.FindElementByCssSelector(".create-save")
        Log("Save Button",, IIf(elBtnSave IsNot Nothing, "Yes", "No"))
        If elBtnSave Is Nothing Then
            allfound = False
        End If

        If allfound Then
            Log("All elements found", , "Yes")

            Log("Try Invalid Values to check Validation")

            Log("Send Invalid keys to abbr", "ERR")
            Log("Check the status of save button", "disabled = [should be true]", elBtnSave.GetAttribute("disabled"))

            Log("Send Invalid keys to ID", "tst")
            elID.SendKeys("tst")
            Log("Value of ID", , elID.GetAttribute("value"))

            Log("Send Valid keys to ID", "233")
            elID.SendKeys("233")
            Log("Value of ID", , elID.GetAttribute("value"))

            Log("Send Invalid keys to abbr", "ERR")
            elAbbr.SendKeys("ERR")
            Log("Value of abbr", , elAbbr.GetAttribute("value"))

            Log("Send Invalid keys to name", "this value for the name of library seems toooooooooooooooo long to fit in................")
            elName.SendKeys("this value for the name of library seems toooooooooooooooo long to fit in................")
            Log("Value of abbr", , elName.GetAttribute("value"))

            Log("Send Invalid keys to abbr", "ERR")
            elAbbr.SendKeys("ERR")
            Log("Value of abbr", , elAbbr.GetAttribute("value"))

            Log("Send Invalid keys to abbr", "ERR")
            Log("Check the status of save button", "disabled = [should be true]", elBtnSave.GetAttribute("disabled"))

            Log("Return to the list")

            elBtnCancel.Click()

        Else
            Log("All elements", , , "Failed to Find All Elements")
        End If



        EndTest("Create New Library With Invalid Values")
    End Sub

    Private Sub CreateNewLibraryWithValidValue(sender As Object, e As RoutedEventArgs)
        BeginTest("Create New Library With Invalid Values")
        ResetURL()

        Dim url = "http://localhost/#/new"

        Log("Navigate to URL: ", url)
        Chrome.Url = url
        Chrome.Navigate()

        Log("wait until the newlib element is visible")
        WaitUntil(ExpectedConditions.ElementIsVisible(By.TagName("newlib")))
        Log("Number of newlib", "Should be 1", Chrome.FindElementsByTagName("newlib").Count)

        Dim allfound As Boolean = True


        Dim elID = Chrome.FindElementByCssSelector("input[name=""id""]")
        Log("Input for id",, IIf(elID IsNot Nothing, "Yes", "No"))
        If elID Is Nothing Then
            allfound = False
        End If

        Dim elAbbr = Chrome.FindElementByCssSelector("input[name=""abbr""]")
        Log("Input for abbr",, IIf(elAbbr IsNot Nothing, "Yes", "No"))
        If elAbbr Is Nothing Then
            allfound = False
        End If

        Dim elName = Chrome.FindElementByCssSelector("input[name=""name""]")
        Log("Input for name",, IIf(elName IsNot Nothing, "Yes", "No"))
        If elName Is Nothing Then
            allfound = False
        End If

        Dim elCampus = Chrome.FindElementByCssSelector("md-select[name=""campus""]")
        Log("Input for campus",, IIf(elCampus IsNot Nothing, "Yes", "No"))
        If elCampus Is Nothing Then
            allfound = False
        End If

        Dim elDesc = Chrome.FindElementByCssSelector("input[name=""desc""]")
        Log("Input for desc",, IIf(elDesc IsNot Nothing, "Yes", "No"))
        If elDesc Is Nothing Then
            allfound = False
        End If

        Dim elURL = Chrome.FindElementByCssSelector("input[name=""url""]")
        Log("Input for url",, IIf(elURL IsNot Nothing, "Yes", "No"))
        If elURL Is Nothing Then
            allfound = False
        End If

        Dim elLat = Chrome.FindElementByCssSelector("input[name=""lat""]")
        Log("Input for latitude",, IIf(elLat IsNot Nothing, "Yes", "No"))
        If elLat Is Nothing Then
            allfound = False
        End If

        Dim elLong = Chrome.FindElementByCssSelector("input[name=""long""]")
        Log("Input for longitude",, IIf(elLong IsNot Nothing, "Yes", "No"))
        If elLong Is Nothing Then
            allfound = False
        End If

        Dim elBtnCancel = Chrome.FindElementByCssSelector(".create-cancel")
        Log("Cancel Button",, IIf(elBtnCancel IsNot Nothing, "Yes", "No"))
        If elBtnCancel Is Nothing Then
            allfound = False
        End If

        Dim elBtnSave = Chrome.FindElementByCssSelector(".create-save")
        Log("Save Button",, IIf(elBtnSave IsNot Nothing, "Yes", "No"))
        If elBtnSave Is Nothing Then
            allfound = False
        End If

        If allfound Then
            Log("All elements found", , "Yes")

            Log("Try Invalid Values to check Validation")

            Log("Send Invalid keys to abbr", "ERR")
            Log("Check the status of save button", "disabled = [should be true]", elBtnSave.GetAttribute("disabled"))


            Log("Send Valid keys to ID", "1234")
            elID.SendKeys("1234")
            Log("Value of ID", , elID.GetAttribute("value"))

            Log("Send Valid keys to abbr", "right")
            elAbbr.SendKeys("right")
            Log("Value of abbr", , elAbbr.GetAttribute("value"))

            Log("Send Valid keys to name", "Name of proper length")
            elName.SendKeys("Name of proper length")
            Log("Value of abbr", , elName.GetAttribute("value"))

            Log("Find all options for Camput")
            Dim options = elCampus.FindElements(By.TagName("md-option"))
            For Each item In options
                Log("Campus option",, item.GetAttribute("value"))
            Next
            If options.Count > 0 Then
                Randomize()
                Dim index = Math.Floor(options.Count * Rnd())
                Dim item = options(Math.Floor(options.Count * Rnd()))
                Log("Select a random option for Campus", "index: " + index.ToString, item.GetAttribute("value"))

                elCampus.Click()
                WaitUntil(ExpectedConditions.ElementToBeClickable(item))
                item.Click()

            End If


            Log("Send Valid keys to desc", "Some Description")
            elDesc.SendKeys("Some Description")

            Log("Send Valid keys to url", "http://www.google.com")
            elURL.SendKeys("http://www.google.com")

            Log("Send Valid keys to lat", "-27.4368155")
            elLong.SendKeys("-27.4368155")

            Log("Send Valid keys to long", "153.0758918")
            elLat.SendKeys("153.0758918")


            Log("Check the status of save button", "disabled = [should be false]", elBtnSave.GetAttribute("disabled"))

            Log("Click save to create new lib", "wait until clickable")

            WaitUntil(ExpectedConditions.ElementToBeClickable(elBtnSave))

            elBtnSave.Click()
        Else
            Log("All elements", , , "Failed to Find All Elements")
        End If



        EndTest("Create New Library With Invalid Values")
    End Sub
End Class
