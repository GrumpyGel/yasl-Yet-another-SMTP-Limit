using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Threading;

//
//    yaslEffectiveLimits : Struct to return effective limits.
//

public struct yaslEffectiveLimits
{
  public int      Day;
  public int      Hour;
  public bool     Allow_Diff_From;
}

//
//    yaslAddress : Struct to return Address split into Account and Domain, separated by '@'.
//

public struct yaslAddress
{
  public string   Account;
  public string   Domain;
}

//
//    yaslDomain : Class used to count how many Addresses for a Domain
//

public class yaslDomain : IComparable<yaslDomain>
{
  public string   Domain      { get; set; }
  public int      Qty         { get; set; }

  public yaslDomain(string p_Domain)
  {
    Domain                  = p_Domain;
    Qty                     = 0;
  }
  
  public int CompareTo(yaslDomain p_Comparison)
  {
    return Domain.CompareTo(p_Comparison.Domain);
  }
}

//
//    yaslOverride : Class holding configuration data per address.
//
//    On Static Config list holds Limits for the Account, Times are not set or used.
//    On Temp Config list, the Times are used and dictate when override applies.
//
//    yaslOverride(XmlElement p_Element) : Constructor Initialises an Override reading data from XML configuration, if the Override can be used XmlValid is set to True.
//    yaslOverride() : constructor used to initialise an overide used to BinarySearch list of overrides.
//    ToXml() : Output to Xml for Admin pages.
//    CompareTo() : Used to Sort the list of overrides
//

public class yaslOverride : IComparable<yaslOverride>
{
  public string     Address             { get; set; }
  public DateTime   TimeStart           { get; set; }    // Only used on Temp, Starting from this Date/Time
  public DateTime   TimeEnd             { get; set; }    // Only used on Temp ,Ending before this Date/Time.
  public int        Limit_Day           { get; set; }
  public int        Limit_Hour          { get; set; }
  public bool       Allow_Diff_From     { get; set; }
  public bool       XmlValid            { get; set; }


  public yaslOverride(XmlElement p_Element)
  {
    string          i_Type              = p_Element.LocalName;

    XmlValid                = false;
    Address                 = yasl.Instance.AttrToString(p_Element,  "Address",       "");
    if (Address.IndexOf("@") == -1) {
      yasl.Instance.AddError("Config", i_Type + " Invalid Address [" + Address + "]");
      return; }
    Limit_Day               = yasl.Instance.AttrToInt(p_Element,    "Limit_Day",        yasl.Instance.Limit_Day);
    Limit_Hour              = yasl.Instance.AttrToInt(p_Element,    "Limit_Hour",       yasl.Instance.Limit_Hour);
    Allow_Diff_From         = yasl.Instance.AttrToBool(p_Element,   "Allow_Diff_From",  yasl.Instance.Allow_Diff_From);
    if (Limit_Day < 0) {
      if (Limit_Day < -1) {
        yasl.Instance.AddError("Config", i_Type + " for [" + Address + "] Limit_Day [" + Limit_Day.ToString() + "] is invalid - Default Limit_Day setting used"); }
      Limit_Day             = yasl.Instance.Limit_Day; }
    if (Limit_Hour < 0) {
      if (Limit_Hour < -1) {
        yasl.Instance.AddError("Config", i_Type + " for [" + Address + "] Limit_Hour [" + Limit_Hour.ToString() + "] is invalid - Default Limit_Hour setting used"); }
      Limit_Hour            = yasl.Instance.Limit_Hour; }
    if (Limit_Day < Limit_Hour) {
      yasl.Instance.AddError("Config", i_Type + " for [" + Address + "] Limit_Hour [" + Limit_Hour.ToString() + "] less than Limit_Day [" + Limit_Day.ToString() + "]  - Limit_Day setting used");
      Limit_Hour            = Limit_Day; }
    if (i_Type == "TempOverride") {
      TimeStart             = yasl.Instance.AttrToDate(p_Element,    "Time_Start",    yasl.DateDefault);
      TimeEnd               = yasl.Instance.AttrToDate(p_Element,    "Time_End",      yasl.DateDefault);
      if (TimeEnd.CompareTo(TimeStart) < 0) {
        yasl.Instance.AddError("Config", i_Type + " for Address [" + Address + "] has Time_End earlier than Time_Start");
        return; }
      if (TimeEnd.CompareTo(DateTime.Now) < 0) {
        yasl.Instance.AddError("Config", i_Type + " for Address [" + Address + "] ignored as before today");
        return; } }
    else {
      TimeStart             = yasl.DateDefault;
      TimeEnd               = yasl.DateDefault; }
    XmlValid                = true;
  }


  public yaslOverride()
  {
    Address                 = "";
    TimeStart               = yasl.DateDefault;
    TimeEnd                 = yasl.DateDefault;
    Limit_Day               = yasl.Instance.Limit_Day;
    Limit_Hour              = yasl.Instance.Limit_Hour;
    Allow_Diff_From         = yasl.Instance.Allow_Diff_From;
    XmlValid                = true;
  }


  public void ToXml(XmlDocument p_Xml, XmlElement p_Element)
  {
    XmlElement      i_Override;

    i_Override              = p_Xml.CreateElement("Override");
    i_Override.SetAttribute("Address",          Address);
    i_Override.SetAttribute("TimeStart",        TimeStart.ToString("dd/MM/yyyy HH:mm"));
    i_Override.SetAttribute("TimeEnd",          TimeEnd.ToString("dd/MM/yyyy HH:mm"));
    i_Override.SetAttribute("Limit_Day",        Limit_Day.ToString());
    i_Override.SetAttribute("Limit_Hour",       Limit_Hour.ToString());
    i_Override.SetAttribute("Allow_Diff_From",  Allow_Diff_From.ToString());
    p_Element.AppendChild(i_Override);
  }


  public int CompareTo(yaslOverride p_Comparison)
  {
    int i_Result            = Address.CompareTo(p_Comparison.Address);
    if (i_Result == 0) {
      i_Result              = TimeStart.CompareTo(p_Comparison.TimeStart); }
    return i_Result;
  }
}

//
//    yaslSent : Main class holding info for each Address that has sent mail.
//
//    yaslSent() : Constructor always passed Address.
//    yaslSent(Element, Gap) : Constructor called when yasl initialises and reads data from file saved when class last destroyed.
//    NewHour() : Method called when the current hour changes and all data must be scrolled by 1 or more hours.
//    Increment() : Method called to record and validate an email send request.
//    ToXml() : Output to Xml to save data when class destroyed and for Admin pages.
//    CompareTo() : Used to Sort the list of yaslSent objects.
//

public class yaslSent : IComparable<yaslSent>
{
  readonly object   c_Lock = new object();
  string            c_Address;
  int               c_Qty_Day;
  int               c_Qty_Hour;
  int               c_Qty_Rejected;
  int               c_Qty_Retained;
  int[]             c_Qty_Earlier       = new int[23];
  bool              c_Warn_Day;
  bool              c_Warn_Hour;


  public string     Address             { get { return c_Address;      } }
  public int        Qty_Day             { get { return c_Qty_Day;      } }
  public int        Qty_Hour            { get { return c_Qty_Hour;     } }


  public yaslSent(string p_Address)
  {
    int             i_Sub;

    c_Address               = p_Address;
    c_Qty_Day               = 0;
    c_Qty_Hour              = 0;
    c_Qty_Rejected          = 0;
    c_Qty_Retained          = 0;
    for (i_Sub = 0; i_Sub < 23; i_Sub++) {
      c_Qty_Earlier[i_Sub]  = 0; }
    c_Warn_Day              = false;
    c_Warn_Hour             = false;
  }


  public yaslSent(XmlElement p_Element, int p_Gap)
  {
    int             i_Rejected;
    int             i_Sub1;
    int             i_Sub2;

    c_Address               = p_Element.GetAttribute("Address");
    c_Qty_Day               = 0;
    c_Qty_Hour              = 0;
    c_Qty_Rejected          = 0;
    c_Qty_Retained          = 0;
    c_Warn_Day              = false;
    c_Warn_Day              = false;
    for (i_Sub1 = 0; i_Sub1 < 23; i_Sub1++) {
      i_Sub2                = i_Sub1 + p_Gap;
      if (i_Sub2 > 23) {
        c_Qty_Earlier[i_Sub1]     = 0; }
      else {
        if (i_Sub2 == 23) {
          c_Qty_Earlier[i_Sub1]   = Int32.Parse(p_Element.GetAttribute("Qty_Hour")); }
        else {
          c_Qty_Earlier[i_Sub1]   = Int32.Parse(p_Element.GetAttribute("Earlier_" + i_Sub2.ToString())); }
        c_Qty_Day                 = c_Qty_Day + c_Qty_Earlier[i_Sub1]; } }
    switch (p_Gap) {
    case 0: c_Qty_Hour            = Int32.Parse(p_Element.GetAttribute("Qty_Hour"));
            c_Qty_Rejected        = Int32.Parse(p_Element.GetAttribute("Qty_Rejected"));
            c_Qty_Retained        = Int32.Parse(p_Element.GetAttribute("Qty_Retained"));
            c_Warn_Day            = ((p_Element.GetAttribute("Warn_Day") == "True") ? true : false);
            c_Warn_Day            = ((p_Element.GetAttribute("Warn_Hour") == "True") ? true : false);
            c_Qty_Day             = c_Qty_Day + c_Qty_Hour;
            break;
    case 1: i_Rejected            = Int32.Parse(p_Element.GetAttribute("Qty_Rejected"));
            if (i_Rejected > yasl.Instance.Retain_Ignore) {
              c_Qty_Retained      = ((i_Rejected - yasl.Instance.Retain_Ignore) * yasl.Instance.Retain_Include) / 100; }
            break; }
  }

  public void NewHour(int p_Gap, yaslEffectiveLimits p_Limits)
  {
    int             i_Qty_Hour;
    bool            i_Warn_Day;
    int             i_Retained;
    int             i_Sub1;
    int             i_Sub2;

    if (p_Gap < 1) {
      return; }
    i_Qty_Hour              = c_Qty_Hour;
    i_Warn_Day              = c_Warn_Day;
    i_Retained              = 0;
    if (p_Gap == 1 && c_Qty_Rejected > yasl.Instance.Retain_Ignore) {
      i_Retained            = ((c_Qty_Rejected - yasl.Instance.Retain_Ignore) * yasl.Instance.Retain_Include) / 100; }
    c_Qty_Day               = 0;
    c_Qty_Hour              = 0;
    c_Qty_Rejected          = 0;
    c_Qty_Retained          = i_Retained;
    c_Warn_Day              = false;
    c_Warn_Hour             = false;
    for (i_Sub1 = 0; i_Sub1 < 23; i_Sub1++) {
      i_Sub2                = i_Sub1 + p_Gap;
      if (i_Sub2 > 23) {
        c_Qty_Earlier[i_Sub1]     = 0; }
      else {
        if (i_Sub2 == 23) {
          c_Qty_Earlier[i_Sub1]   = i_Qty_Hour; }
        else {
          c_Qty_Earlier[i_Sub1]   = c_Qty_Earlier[i_Sub2]; }
        c_Qty_Day                 = c_Qty_Day + c_Qty_Earlier[i_Sub1]; } }
    if (i_Warn_Day == true) {
      if (c_Qty_Day > ((p_Limits.Day * yasl.Instance.Limit_Warn_Clear) / 100)) {
        c_Warn_Day                = true; } }
  }

  public int Increment(XmlElement p_Root, int p_Qty, bool p_FromSame, yaslEffectiveLimits p_Limits)
  {
    int             i_Result            = yasl.Result_Allow;
    int             i_Qty_Day           = 0;
    int             i_Qty_Hour          = 0;
    bool            i_Warn              = false;

    lock (c_Lock) {
      while (true) {
        if (p_Qty < 1) {
          i_Result          = yasl.Result_Decline_Qty;
          break; }
        if (p_FromSame == false && p_Limits.Allow_Diff_From == false) {
          i_Result          = yasl.Result_Decline_From;
          break; }
        i_Qty_Day           = c_Qty_Day + c_Qty_Retained + p_Qty;
        i_Qty_Hour          = c_Qty_Hour + c_Qty_Retained + p_Qty;
        if (i_Qty_Day > p_Limits.Day) {
          c_Qty_Rejected++;
          i_Result          = yasl.Result_Decline_Day;
          break; }
        if (i_Qty_Hour > p_Limits.Hour) {
          c_Qty_Rejected++;
          i_Result          = yasl.Result_Decline_Hour;
          break; }
        c_Qty_Day           = c_Qty_Day + p_Qty;
        c_Qty_Hour          = c_Qty_Hour + p_Qty;
        if (i_Qty_Day > ((p_Limits.Day * yasl.Instance.Limit_Warn) / 100) && c_Warn_Day == false) {
          c_Warn_Day        = true;
          i_Warn            = true; }
        if (i_Qty_Hour > ((p_Limits.Hour * yasl.Instance.Limit_Warn) / 100) && c_Warn_Hour == false) {
          c_Warn_Hour       = true;
          i_Warn            = true; }
        if (i_Warn == false) {
          break; }
        p_Root.SetAttribute("Limit_Day",   p_Limits.Day.ToString());
        p_Root.SetAttribute("Limit_Hour",  p_Limits.Hour.ToString());
        p_Root.SetAttribute("Qty_Day",     i_Qty_Day.ToString());
        p_Root.SetAttribute("Qty_Hour",    i_Qty_Hour.ToString());
        if (c_Warn_Day == true) {
          if (c_Warn_Hour == true) {
            i_Result        = yasl.Result_Warn_Both; }
          else {
            i_Result        = yasl.Result_Warn_Day; } }
        else {
          i_Result          = yasl.Result_Warn_Hour; }
        break; } }
    return i_Result;
  }

  public void ToXml(XmlElement p_Element)
  {
    int    i_Sub;

    p_Element.SetAttribute("Address",       c_Address);
    p_Element.SetAttribute("Qty_Day",       c_Qty_Day.ToString());
    p_Element.SetAttribute("Qty_Hour",      c_Qty_Hour.ToString());
    p_Element.SetAttribute("Qty_Rejected",  c_Qty_Rejected.ToString());
    p_Element.SetAttribute("Qty_Retained",  c_Qty_Retained.ToString());
    p_Element.SetAttribute("Warn_Day",      c_Warn_Day.ToString());
    p_Element.SetAttribute("Warn_Hour",     c_Warn_Hour.ToString());
    for (i_Sub = 0; i_Sub < 23; i_Sub++) {
      p_Element.SetAttribute("Earlier_" + i_Sub.ToString(),  c_Qty_Earlier[i_Sub].ToString()); }
  }

  public void ToXml(XmlElement p_Element, yaslEffectiveLimits p_Limits)
  {
    int             i_Sub;
    bool            i_Over_Day          = false;
    bool            i_Over_Hour         = false;

    if ((c_Qty_Day + c_Qty_Retained + 1) > p_Limits.Day) {
      i_Over_Day            = true; }
    if ((c_Qty_Hour + c_Qty_Retained + 1) > p_Limits.Hour) {
      i_Over_Hour           = true; }
    ToXml(p_Element);
    p_Element.SetAttribute("Limit_Day",        p_Limits.Day.ToString());
    p_Element.SetAttribute("Limit_Hour",       p_Limits.Hour.ToString());
    p_Element.SetAttribute("Over_Day",         i_Over_Day.ToString());
    p_Element.SetAttribute("Over_Hour",        i_Over_Hour.ToString());
    p_Element.SetAttribute("Allow_Diff_From",  p_Limits.Allow_Diff_From.ToString());
  }

  public int CompareTo(yaslSent p_Comparison)
  {
    return c_Address.CompareTo(p_Comparison.Address);
  }
}

//
//    =============================================================
//    yasl : Main Singleton Object Class administering Smpt Limits.
//    =============================================================
//
//    Config Options:
//      Limit_Day         Number of emails (all recipients counted individually) an account is allowed to send per rolling 24 hour period.
//      Limit_Hour        Number of emails (all recipients counted individually) an account is allowed to send in any 1 hour (not rolling, ie from :00 to :59).
//      Limit_Warn        Once this percentage of the Daily or Hourly Limit is reached, a warning email will be sent to the account.  Only 1 is sent, subsequent emails up to the Limit will not receive a warning.
//      Limit_Warn_Clear  For the Daily warning, if a warning has been sent, at the end of each hour, the percentage will be rechecked and if less than this a warning will be resent if the warning limit is exceeded again.
//      Retain_Include    To stop hackers being able to send more spam as a new hour is reached, this percentage of Rejections from the previous hour will be carried forward as 'sent' emails which will likely take it back over the allowable limit.
//      Retain_Ignore     Hackers usually will send 100s of emails per hour, where as a legitimate user, much less.  This number of Rejections from the previous hour will therefore not be carried forward and therefore likely allow a user back into his account.
//      Allow_Diff_From   If set to True, the From address in an email can be different to the Authenticated login account.  If False, any emails with a From address different to the Autheicated login account will be rejected.
//      Max_Errors        The maximum number of errors to log, the log will stop accumulating errors when this is reached.
//
//    yasl_Config.xml:
//      This file holds the Config options and overrides for each account.
//      System wide settings are set as attributes on the <yasl> document element.
//      Overrides for each account should be added as a <StaticOverride> element and may have Limit_Day, Limit_Hour and Allow_Diff_From settings.  When not provided, the system setting is used.
//      The following sample file sets all the system options to their default values and sets all overrides for account1@example.com and the Limit_Day only for account2@example.com:
//
//        <yasl Limit_Day="200" Limit_Hour="50" Limit_Warn="80" Limit_Warn_Clear="60" Retain_Ignore="10" Retain_Include="30" Allow_Diff_From="False" Max_Errors="500">
//          <StaticOverride Address="account1@example.com" Limit_Day="30" Limit_Hour="10" Allow_Diff_From="True"/>
//          <StaticOverride Address="account2@example.com" Limit_Day="100"/>
//        </yasl>
//
//    yasl_Overrides.xml:
//      This file holds overrides per account that operate between start and end times.
//      Overrides for each account should be added as a <TempOverride> element and may have Limit_Day, Limit_Hour and Allow_Diff_From settings.  When not provided, the system setting is used.
//      The start and end times are set using Time_Start and Time_End elements.
//      The following sample file sets the Daily and Hourly Limits for account2@example.com active 1st Feb 2021 through 5th Feb 2021:
//
//        <yasl>
//          <TempOverride Address="account2@example.com" Time_Start="2021-02-01 10:00" Time_End="2021-02-06 00:00" Limit_Day="300" Limit_Hour="100"/>
//        </yasl>
//
//    ~yasl() : Method called when object is destroyed so that data is saved to be reloaded when the object is next created.
//    NewMessage() : Method called when a email send is requested, always passed Authorised Login Address, Qty of recipients and whether the From address is the same as the login.
//
//    Methods called by the Admin pages:
//      GetStatus() : Return current status of the object.
//      GetConfig() : Return Config data loaded.
//      GetSent() : Return list of Addresses that have reqquested email to be sent.
//      ManualSaveSent() : Save the Sent Data file, was only useful in development.
//      ManualLoadConfig() : Reload the Config files, for example after editing them for changes to take effect.
//      ManualClearErrors() : Clear the Error log.
//
//    Private Methods called from public ones:
//      InitialiseData() : Called at top of all public methods to ensure object is initialised and has config & previous data loaded.
//      InitialiseData_Config() : Called by InitialiseData to load XML config files.
//      InitialiseData_Sent() : Called by InitialiseData to load most recent sent data file.
//      CheckTime() : Called by public methods after InitialiseData to check if the Hour has changed.  If so will save the current data and roll on to current hour (may roll on by more than 1 hour if light usage, eg night time).
//      SaveSent() : Saves the current list of addresses that have sent mail.  This may be reloaded if the object is recreated.
//      GetEffectiveLimits() : Is passed an Address and will analyse the Config data to find this Address's Limits.
//      SplitAddress() : Splits the passed email Addess into Account and Domain parts.
//
//    Public utility Methods:
//      AddError() : Records an error into the error log.  ** Must be within a WriteLock to call this **.
//      AttrTo<DataType>() : Safe method to return an Xml Attribute from an Element.
//
public class yasl
{
  private static readonly yasl      s_Instance       = new yasl();
  private static readonly DateTime  s_DateDefault    = new DateTime(2020, 1, 1, 0, 0, 0);
  
  private const int        s_LockTimeoutWrite  = 1000;
  private const int        s_LockTimeoutRead   = 1000;

  public static yasl       Instance         { get { return s_Instance;    } }
  public static DateTime   DateDefault      { get { return s_DateDefault;  } }

  ReaderWriterLock   c_Rwl              = new ReaderWriterLock();

  int                c_Initialised      = 0;

  int                c_Limit_Day            = 200;
  int                c_Limit_Hour           = 50;
  int                c_Limit_Warn           = 80;
  int                c_Limit_Warn_Clear     = 60;
  int                c_Retain_Ignore        = 10;
  int                c_Retain_Include       = 30;
  bool               c_Allow_Diff_From      = false;
  int                c_Max_Errors           = 500;
  bool               c_Error_Hour_Reported  = false;
  List<yaslOverride> c_StaticOverrides      = new List<yaslOverride>();
  List<yaslOverride> c_TempOverrides        = new List<yaslOverride>();
  List<String>       c_Errors               = new List<String>();

  DateTime           c_BaseDate;
  long               c_BaseTicks            = 0;
  DateTime           c_CurrDate             = new DateTime(1, 1, 1, 0, 0, 0);
  int                c_CurrHour             = 0;
  int                c_LimitHour            = 0;
  List<yaslSent>     c_Sent                 = new List<yaslSent>();

  public const int  Result_Allow            = 0;
  public const int  Result_Warn_Day         = 10;
  public const int  Result_Warn_Hour        = 11;
  public const int  Result_Warn_Both        = 12;
  public const int  Result_Decline_Day      = 20;
  public const int  Result_Decline_Hour     = 21;
  public const int  Result_Decline_Both     = 22;
  public const int  Result_Decline_Qty      = 30;
  public const int  Result_Decline_From     = 31;
  public const int  Result_Error            = 99;

  public const string  yasl_DataPath        = "E:\\web\\wwwroot\\Private\\";


  public int        Limit_Day           { get { return c_Limit_Day;         } }
  public int        Limit_Hour          { get { return c_Limit_Hour;        } }
  public int        Limit_Warn          { get { return c_Limit_Warn;        } }
  public int        Limit_Warn_Clear    { get { return c_Limit_Warn_Clear;  } }
  public int        Retain_Ignore       { get { return c_Retain_Ignore;     } }
  public int        Retain_Include      { get { return c_Retain_Include;    } }
  public bool       Allow_Diff_From     { get { return c_Allow_Diff_From;   } }
  public int        Max_Errors          { get { return c_Max_Errors;        } }


  static yasl() { }
  private yasl() { }


  ~yasl()
  {
    SaveSent();
  }


  public int NewMessage(XmlElement p_Root, string p_Address, int p_Qty, bool p_FromSame)
  {
    LockCookie          i_WriteLock;
    yaslEffectiveLimits i_Limits;
    yaslSent            i_FindSent;
    DateTime            i_Now           = DateTime.Now;
    int                 i_Result        = 0;
    int                 i_Idx           = 0;

    try {
      c_Rwl.AcquireReaderLock(s_LockTimeoutRead);
      try {
        if (c_Initialised == 0) {
          InitialiseData(); }
        CheckTime(i_Now);
        i_Limits            = GetEffectiveLimits(i_Now, p_Address);
        i_FindSent          = new yaslSent(p_Address);
        i_Idx               = c_Sent.BinarySearch(i_FindSent);
        if (i_Idx < 0) {
          i_WriteLock       = c_Rwl.UpgradeToWriterLock(s_LockTimeoutWrite);
          try {
            i_Idx           = c_Sent.BinarySearch(i_FindSent);
            if (i_Idx < 0) {
              i_Idx         = ~i_Idx;
              c_Sent.Insert(i_Idx, i_FindSent); } }
          finally {
            c_Rwl.DowngradeFromWriterLock(ref i_WriteLock); } }
        i_Result            = c_Sent[i_Idx].Increment(p_Root, p_Qty, p_FromSame, i_Limits); }
      finally {
        c_Rwl.ReleaseReaderLock(); } }
    catch(Exception ex) {
      try {
        c_Rwl.AcquireWriterLock(s_LockTimeoutWrite);
        try {
          AddError("Message", "Exception : " + ex.Message); }
        finally {
          c_Rwl.ReleaseWriterLock(); } }
      catch(Exception ex2) { }
      i_Result              = Result_Error; }
    return i_Result;
  }


  public string GetStatus(XmlElement p_Root)
  {
    XmlDocument     i_Xml;
    XmlElement      i_Group;
    XmlElement      i_Item;
    string          i_Result            = "";
    int             i_Idx;

    try {
      i_Xml                 = p_Root.OwnerDocument;
      c_Rwl.AcquireReaderLock(s_LockTimeoutRead);
      try {
        if (c_Initialised == 0) {
          InitialiseData(); }
        CheckTime(DateTime.Now);
        p_Root.SetAttribute("CurrDate",      c_CurrDate.ToString("dd/MM/yyyy"));
        p_Root.SetAttribute("CurrHour",      c_CurrHour.ToString());
        p_Root.SetAttribute("CurrHourDisp",  ((c_CurrHour < 12) ? c_CurrHour.ToString() + "am" : ((c_CurrHour == 12) ? "12pm" : (c_CurrHour - 12).ToString() + "pm")));
        p_Root.SetAttribute("LimitHour",     c_LimitHour.ToString());
        p_Root.SetAttribute("QtyStatic",     c_StaticOverrides.Count.ToString());
        p_Root.SetAttribute("QtyTemp",       c_TempOverrides.Count.ToString());
        p_Root.SetAttribute("QtySent",       c_Sent.Count.ToString());
        p_Root.SetAttribute("QtyErrors",     c_Errors.Count.ToString());
        if (c_Errors.Count > 0) {
          i_Group             = i_Xml.CreateElement("Errors");
          for (i_Idx = 0; i_Idx < c_Errors.Count; i_Idx++) {
            i_Item            = i_Xml.CreateElement("Error");
            i_Item.InnerText  = c_Errors[i_Idx];
            i_Group.AppendChild(i_Item); }
          p_Root.AppendChild(i_Group); }
        i_Result              = "OK"; }
      finally {
        c_Rwl.ReleaseReaderLock(); } }
    catch(Exception ex) {
      i_Result                = ex.Message; }
    return i_Result;
  }


  public string GetConfig(XmlElement p_Root)
  {
    XmlDocument     i_Xml;
    XmlElement      i_Config;
    XmlElement      i_Group;
    string          i_Result            = "";
    int             i_Idx               = 0;

    try {
      i_Xml                 = p_Root.OwnerDocument;
      c_Rwl.AcquireReaderLock(s_LockTimeoutRead);
      try {
        if (c_Initialised == 0) {
          InitialiseData(); }
        CheckTime(DateTime.Now);
        i_Config            = i_Xml.CreateElement("Config");
        i_Config.SetAttribute("Limit_Day",         c_Limit_Day.ToString());
        i_Config.SetAttribute("Limit_Hour",        c_Limit_Hour.ToString());
        i_Config.SetAttribute("Limit_Warn",        c_Limit_Warn.ToString());
        i_Config.SetAttribute("Limit_Warn_Clear",  c_Limit_Warn_Clear.ToString());
        i_Config.SetAttribute("Retain_Ignore",     c_Retain_Ignore.ToString());
        i_Config.SetAttribute("Retain_Include",    c_Retain_Include.ToString());
        i_Config.SetAttribute("Allow_Diff_From",   c_Allow_Diff_From.ToString());
        i_Config.SetAttribute("Max_Errors",        c_Max_Errors.ToString());
        i_Group             = i_Xml.CreateElement("Static");
        for (i_Idx = 0; i_Idx < c_StaticOverrides.Count; i_Idx++) {
          c_StaticOverrides[i_Idx].ToXml(i_Xml, i_Group); }
        i_Config.AppendChild(i_Group);
        i_Group             = i_Xml.CreateElement("Temp");
        for (i_Idx = 0; i_Idx < c_TempOverrides.Count; i_Idx++) {
          c_TempOverrides[i_Idx].ToXml(i_Xml, i_Group); }
        i_Config.AppendChild(i_Group);
        p_Root.AppendChild(i_Config);
        i_Result            = "OK"; }
      finally {
        c_Rwl.ReleaseReaderLock(); } }
    catch(Exception ex) {
      i_Result              = ex.Message; }
    return i_Result;
  }


  public string GetSent(XmlElement p_Root)
  {
    XmlDocument          i_Xml;
    XmlElement           i_Group;
    XmlElement           i_Item;
    yaslEffectiveLimits  i_Limits;
    yaslAddress          i_SplitAddress;
    yaslDomain           i_Domain;
    List<yaslDomain>     i_Domains      = new List<yaslDomain>();
    DateTime             i_Now          = DateTime.Now;
    int                  i_Hour;
    string               i_Address      = "";
    string               i_Result       = "";
    int                  i_Idx          = 0;
    int                  i_Idx2         = 0;

    try {
      i_Xml                 = p_Root.OwnerDocument;
      c_Rwl.AcquireReaderLock(s_LockTimeoutRead);
      try {
        if (c_Initialised == 0) {
          InitialiseData(); }
        CheckTime(DateTime.Now);
        i_Group             = i_Xml.CreateElement("Sent");
        i_Hour              = c_CurrHour;
        for (i_Idx = 0; i_Idx < 23; i_Idx++) {
          i_Hour            = ((i_Hour < 23) ? i_Hour + 1 : 0);
          i_Group.SetAttribute("Hour_" + i_Idx.ToString(),  ((i_Hour < 12) ? i_Hour.ToString() + "am" : ((i_Hour == 12) ? "12pm" : (i_Hour - 12).ToString() + "pm"))); }
        for (i_Idx = 0; i_Idx < c_Sent.Count; i_Idx++) {
          i_Address         = c_Sent[i_Idx].Address;
          i_Limits          = GetEffectiveLimits(i_Now, i_Address);
          i_SplitAddress    = SplitAddress(i_Address);
          i_Domain          = new yaslDomain(i_SplitAddress.Domain);
          i_Idx2            = i_Domains.BinarySearch(i_Domain);
          if (i_Idx2 < 0) {
            i_Idx2          = ~i_Idx2;
            i_Domains.Insert(i_Idx2, i_Domain); }
          i_Domains[i_Idx2].Qty++;
          i_Item            = i_Xml.CreateElement("Account");
          i_Item.SetAttribute("Account",   i_SplitAddress.Account);
          i_Item.SetAttribute("Domain",    i_SplitAddress.Domain);
          c_Sent[i_Idx].ToXml(i_Item, i_Limits);
          i_Group.AppendChild(i_Item); }
        p_Root.AppendChild(i_Group);

        i_Group             = i_Xml.CreateElement("Domains");
        for (i_Idx = 0; i_Idx < i_Domains.Count; i_Idx++) {
          i_Item            = i_Xml.CreateElement("Domain");
          i_Item.SetAttribute("Domain",   i_Domains[i_Idx].Domain);
          i_Item.SetAttribute("Qty",      i_Domains[i_Idx].Qty.ToString());
          i_Group.AppendChild(i_Item); }
        p_Root.AppendChild(i_Group);

        i_Result            = "OK"; }
      finally {
        c_Rwl.ReleaseReaderLock(); } }
    catch(Exception ex) {
      i_Result              = ex.Message; }
    return i_Result;
  }


  public string ManualSaveSent()
  {
    string          i_Result            = "";

    try {
      c_Rwl.AcquireReaderLock(s_LockTimeoutRead);
      try {
        if (c_Initialised == 0) {
          InitialiseData(); }
        CheckTime(DateTime.Now);
        SaveSent();
        i_Result            = "OK"; }
      finally {
        c_Rwl.ReleaseReaderLock(); } }
    catch(Exception ex) {
      i_Result              = ex.Message; }
    return i_Result;
  }


  public string ManualLoadConfig()
  {
    LockCookie      i_WriteLock;
    string          i_Result            = "";

    try {
      c_Rwl.AcquireReaderLock(s_LockTimeoutRead);
      try {
        if (c_Initialised == 0) {
          InitialiseData(); }
        else {
          i_WriteLock       = c_Rwl.UpgradeToWriterLock(s_LockTimeoutWrite);
          try {
            InitialiseData_Config(); }
          finally {
            c_Rwl.DowngradeFromWriterLock(ref i_WriteLock); } }
        CheckTime(DateTime.Now);
        i_Result            = "OK"; }
      finally {
        c_Rwl.ReleaseReaderLock(); } }
    catch(Exception ex) {
      i_Result              = ex.Message; }
    return i_Result;
  }


  public string ManualClearErrors()
  {
    LockCookie      i_WriteLock;
    string          i_Result            = "";

    try {
      c_Rwl.AcquireReaderLock(s_LockTimeoutRead);
      try {
        if (c_Initialised == 0) {
          InitialiseData(); }
        else {
          i_WriteLock       = c_Rwl.UpgradeToWriterLock(s_LockTimeoutWrite);
          try {
            c_Errors.Clear(); }
          finally {
            c_Rwl.DowngradeFromWriterLock(ref i_WriteLock); } }
        CheckTime(DateTime.Now);
        i_Result            = "OK"; }
      finally {
        c_Rwl.ReleaseReaderLock(); } }
    catch(Exception ex) {
      i_Result              = ex.Message; }
    return i_Result;
  }


  private void InitialiseData()
  {
    LockCookie      i_WriteLock;

    i_WriteLock             = c_Rwl.UpgradeToWriterLock(s_LockTimeoutWrite);
    try {
      if (c_Initialised == 0) {
        c_Errors.Clear();
        InitialiseData_Config();
        c_BaseDate          = new DateTime(2020, 1, 1);
        c_BaseTicks         = c_BaseDate.Ticks;
        c_CurrDate          = DateTime.Now;
        c_CurrHour          = c_CurrDate.Hour;
        c_LimitHour         = (int) ((c_CurrDate.Ticks - c_BaseTicks) / TimeSpan.TicksPerHour);
        c_CurrDate          = c_CurrDate.Date;
        c_Error_Hour_Reported  = false;
        InitialiseData_Sent();
        c_Initialised       = 1; } }
    finally {
      c_Rwl.DowngradeFromWriterLock(ref i_WriteLock); }
  }



  private void InitialiseData_Config()
  {
    XmlDocument     i_Doc;
    XmlNodeList     i_Overrides;
    XmlElement      i_Element;
    yaslOverride    i_Override;
    yaslOverride    i_Static;
    int             i_Sub;

    try {
      c_StaticOverrides.Clear();
      c_TempOverrides.Clear();
      i_Doc = new XmlDocument();
      i_Doc.Load(yasl_DataPath + "yasl_Config.xml");
      i_Element             = i_Doc.DocumentElement;
      c_Limit_Day           = AttrToInt(i_Element,  "Limit_Day",    200);
      c_Limit_Hour          = AttrToInt(i_Element,  "Limit_Hour",    50);
      c_Limit_Warn          = AttrToInt(i_Element,  "Limit_Warn",    80);
      c_Limit_Warn_Clear    = AttrToInt(i_Element,  "Limit_Warn_Clear",  60);
      c_Retain_Ignore       = AttrToInt(i_Element,  "Retain_Ignore",  10);
      c_Retain_Include      = AttrToInt(i_Element,  "Retain_Include",  30);
      c_Allow_Diff_From     = AttrToBool(i_Element,  "Allow_Diff_From",  false);
      c_Max_Errors          = AttrToInt(i_Element,  "Max_Errors",    500);
      if (c_Limit_Day < c_Limit_Hour) {
        AddError("Config", "Limit_Day [" + c_Limit_Day.ToString() + "] less than Limit_Hour [" + c_Limit_Hour.ToString() + "]  - Limit_Hour setting used");
        c_Limit_Day         = c_Limit_Hour; }
      if (c_Limit_Warn_Clear > c_Limit_Warn) {
        AddError("Config", "Limit_Warn_Clear [" + c_Limit_Warn_Clear.ToString() + "] greater than Limit_Warn [" + c_Limit_Warn.ToString() + "]  - Limit_Warn setting used");
        c_Limit_Warn_Clear  = c_Limit_Warn; }

      i_Overrides           = i_Element.GetElementsByTagName("StaticOverride");
      for (i_Sub=0; i_Sub < i_Overrides.Count; i_Sub++) {
        i_Element           = (XmlElement) i_Overrides[i_Sub];
        i_Override          = new yaslOverride(i_Element);
        if (i_Override.XmlValid == true) {
          c_StaticOverrides.Add(i_Override); } }
      c_StaticOverrides.Sort();
      for (i_Sub=0; i_Sub < (c_StaticOverrides.Count - 1); i_Sub++) {
        if (c_StaticOverrides[i_Sub].Address == c_StaticOverrides[i_Sub + 1].Address) {
          AddError("Config", "Duplicate Static Override for " + c_StaticOverrides[i_Sub].Address);
          c_StaticOverrides.RemoveAt(i_Sub + 1); } }
      i_Doc = new XmlDocument();
      i_Doc.Load(yasl_DataPath + "yasl_Overrides.xml");
      i_Overrides           = i_Doc.DocumentElement.GetElementsByTagName("TempOverride");
      for (i_Sub=0; i_Sub < i_Overrides.Count; i_Sub++) {
        i_Element           = (XmlElement) i_Overrides[i_Sub];
        i_Override          = new yaslOverride(i_Element);
        if (i_Override.XmlValid == true) {
          c_TempOverrides.Add(i_Override); } }
      c_TempOverrides.Sort();
      for (i_Sub=0; i_Sub < (c_TempOverrides.Count - 1); i_Sub++) {
        if (c_TempOverrides[i_Sub].Address == c_TempOverrides[i_Sub + 1].Address) {
          if (c_TempOverrides[i_Sub + 1].TimeStart.CompareTo(c_TempOverrides[i_Sub].TimeEnd) < 0) {
            AddError("Config", "Temp Override overlaps " + c_StaticOverrides[i_Sub].Address + " starting " + c_TempOverrides[i_Sub + 1].TimeStart.ToString("dd/MM/yyyy HH:mm"));
            c_TempOverrides.RemoveAt(i_Sub + 1); } } } }
    catch(Exception ex) {
      AddError("Config", "Exception - " + ex.Message); }
  }


  private void InitialiseData_Sent()
  {
    XmlDocument     i_Doc;
    XmlNodeList     i_Nodes;
    XmlElement      i_Element;
    yaslSent        i_Sent;
    string          i_Path;
    string          i_Filename;
    string          i_Address;
    int             i_Hour;
    bool            i_Checking;
    bool            i_Found;
    int             i_Gap;
    int             i_Idx;
    int             i_Sub;

    i_Path                  = yasl_DataPath + "yasl_Data";
    i_Filename              = "";
    if (Directory.Exists(i_Path) == false) {
      AddError("LoadSent", "No Data directory found - should automatically be created if this is the first run.");
      return; }
    i_Doc                   = new XmlDocument();
    i_Hour                  = c_LimitHour + 1;
    i_Checking              = true;
    i_Found                 = false;
    while (i_Checking == true) {
      i_Hour--;
      if (i_Hour < c_LimitHour - 23) {
        i_Checking          = false; }
      else {
        try {
          i_Filename        = "yasl_data_" + i_Hour.ToString() + ".xml";
          i_Doc.Load(i_Path + "\\" + i_Filename);
          i_Found           = true;
          i_Checking        = false; }
        catch (Exception) { } } }
    if (i_Found == false) {
      AddError("LoadSent", "No sent data found for last 24 hours.");
      return; }
    i_Gap                   = c_LimitHour - i_Hour;
    i_Nodes                 = i_Doc.DocumentElement.GetElementsByTagName("Sent");
    if (i_Nodes.Count != 1) {
      return; }
    i_Element               = (XmlElement) i_Nodes[0];
    if (AttrToInt(i_Element, "LimitHour", 0) != i_Hour) {
      AddError("LoadSent", "File LimitHour mismatch [" + AttrToInt(i_Element, "LimitHour", 0).ToString() + "] [" + i_Hour.ToString() + "] no data loaded from file " + i_Filename);
      return; }
    i_Nodes                 = i_Element.GetElementsByTagName("Account");
    for (i_Sub=0; i_Sub < i_Nodes.Count; i_Sub++) {
      i_Element             = (XmlElement) i_Nodes[i_Sub];
//    i_Address             = AttrToString(i_Element, "Address", "");
//    i_Sent                = new yaslSent(i_Address);
      i_Sent                = new yaslSent(i_Element, i_Gap);
      i_Idx                = c_Sent.BinarySearch(i_Sent);
      if (i_Idx < 0) {
        i_Idx               = ~i_Idx;
//        i_Sent.Set(i_Element, i_Gap);
        c_Sent.Insert(i_Idx, i_Sent); }
      else {
        AddError("LoadSent", "Duplicate record found for " + i_Sent.Address); } }
  }


  private void CheckTime(DateTime p_Time)
  {
    LockCookie           i_WriteLock;
    yaslEffectiveLimits  i_Limits;
    int                  i_LimitHour;
    int                  i_Gap;
    int                  i_Idx;

    i_LimitHour             = (int) ((p_Time.Ticks - c_BaseTicks) / TimeSpan.TicksPerHour);
    if (i_LimitHour == c_LimitHour) {
      return; }

    i_WriteLock             = c_Rwl.UpgradeToWriterLock(s_LockTimeoutWrite);
    try {
      while (true) {
        i_LimitHour               = (int) ((p_Time.Ticks - c_BaseTicks) / TimeSpan.TicksPerHour);
        if (i_LimitHour < c_LimitHour) {
          if (c_Error_Hour_Reported == false) {
            AddError("CheckTime", "Calculated LimitHour [" + i_LimitHour.ToString() + "] less than Current LimitHour [" + c_LimitHour.ToString() + "]");
            c_Error_Hour_Reported = true; }
          break; }
        if (i_LimitHour == c_LimitHour) {
          break; }
        try {
          SaveSent(); }
        catch (Exception) { }
        i_Gap                     = i_LimitHour - c_LimitHour;
        c_CurrDate                = p_Time.Date;
        c_CurrHour                = p_Time.Hour;
        c_LimitHour               = i_LimitHour;
        c_Error_Hour_Reported     = false;
        if (i_Gap > 23) {
          c_Sent.Clear();
          break; }
        i_Idx                     = 0;
        while (i_Idx < c_Sent.Count) {
          i_Limits                = GetEffectiveLimits(p_Time, c_Sent[i_Idx].Address);
          c_Sent[i_Idx].NewHour(i_Gap, i_Limits);
          if (c_Sent[i_Idx].Qty_Day == 0) {
            c_Sent.RemoveAt(i_Idx); }
          else {
            i_Idx++; } }
        break; } }
    finally {
      c_Rwl.DowngradeFromWriterLock(ref i_WriteLock); }
  }


  private void SaveSent()
  {
    XmlDocument     i_Xml;
    XmlElement      i_Root;
    XmlElement      i_Group;
    XmlElement      i_Item;
    string          i_Path;
    string          i_Filename;
    int             i_Idx;

    i_Xml                   = new XmlDocument();
    i_Root                  = i_Xml.CreateElement("root");
    i_Group                 = i_Xml.CreateElement("Sent");
    i_Group.SetAttribute("Date",       c_CurrDate.ToString("dd/MM/yyyy"));
    i_Group.SetAttribute("Hour",       c_CurrHour.ToString());
    i_Group.SetAttribute("LimitHour",  c_LimitHour.ToString());
    for (i_Idx = 0; i_Idx < c_Sent.Count; i_Idx++) {
      i_Item                = i_Xml.CreateElement("Account");
      c_Sent[i_Idx].ToXml(i_Item);
      i_Group.AppendChild(i_Item); }
    i_Root.AppendChild(i_Group);
    i_Xml.AppendChild(i_Root);
    i_Path                  = yasl_DataPath + "yasl_Data";
    i_Filename              = "yasl_data_" + c_LimitHour.ToString() + ".xml";
    if (Directory.Exists(i_Path) == false) {
      Directory.CreateDirectory(i_Path); }
    i_Xml.Save(i_Path + "\\" + i_Filename);
  }


  private yaslEffectiveLimits GetEffectiveLimits(DateTime p_Now, string p_Address)
  {
    yaslEffectiveLimits  i_Results      = new yaslEffectiveLimits();
    yaslOverride         i_FindOverride = new yaslOverride();
    int                  i_Idx;

    i_Results.Day                 = c_Limit_Day;
    i_Results.Hour                = c_Limit_Hour;
    i_Results.Allow_Diff_From     = c_Allow_Diff_From;
    i_FindOverride.Address        = p_Address;
    i_Idx                         = c_StaticOverrides.BinarySearch(i_FindOverride);
    if (i_Idx >= 0) {
      i_Results.Allow_Diff_From   = c_StaticOverrides[i_Idx].Allow_Diff_From;
      if (c_StaticOverrides[i_Idx].Limit_Day >= 0) {
        i_Results.Day             = c_StaticOverrides[i_Idx].Limit_Day; }
      if (c_StaticOverrides[i_Idx].Limit_Hour >= 0) {
        i_Results.Hour            = c_StaticOverrides[i_Idx].Limit_Hour; } }
    i_FindOverride.TimeStart      = p_Now;
    i_Idx                         = c_TempOverrides.BinarySearch(i_FindOverride);
    if (i_Idx < 0) {
      i_Idx                       = ~i_Idx - 1;
      if (i_Idx >= 0) {
        if (c_TempOverrides[i_Idx].Address != p_Address || c_TempOverrides[i_Idx].TimeStart.CompareTo(p_Now) < 0 || c_TempOverrides[i_Idx].TimeEnd.CompareTo(p_Now) <= 0) {
          i_Idx                   = -1; } } }
    if (i_Idx >= 0) {
      i_Results.Allow_Diff_From   = c_TempOverrides[i_Idx].Allow_Diff_From;
      if (c_TempOverrides[i_Idx].Limit_Day >= 0) {
        i_Results.Day             = c_TempOverrides[i_Idx].Limit_Day; }
      if (c_TempOverrides[i_Idx].Limit_Hour >= 0) {
        i_Results.Hour            = c_TempOverrides[i_Idx].Limit_Hour; } }
    return i_Results;
  }


  private yaslAddress SplitAddress(string p_Address)
  {
    yaslAddress     i_Results           = new yaslAddress();
    int             i_Idx;

    i_Idx                   = p_Address.IndexOf("@");
    if (i_Idx == -1) {
      i_Results.Account     = p_Address;
      i_Results.Domain      = ""; }
    else {
      i_Results.Account     = p_Address.Substring(0, i_Idx);
      i_Results.Domain      = p_Address.Substring(i_Idx + 1); }
    return i_Results;
  }

//
//    Must be in write lock to call this
//

  public void AddError(string p_Type, string p_Message)
  {
    try {
      if (c_Errors.Count < c_Max_Errors) {
        c_Errors.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " " + p_Type + " : " + p_Message); } }
    catch (Exception ex) { }
  }

//
//    Routines to safely extract XML Attribute values
//

  public string AttrToString(XmlElement p_Ele, string p_Name, string p_Default)
  {
    string          i_Value             = p_Default;

    try {
      if (p_Ele.HasAttribute(p_Name)) {
        i_Value             = p_Ele.GetAttribute(p_Name); } }
    catch (Exception ex) {
      i_Value               = p_Default; }
    return i_Value;
  }


  public int AttrToInt(XmlElement p_Ele, string p_Name, int p_Default)
  {
    int             i_Value             = p_Default;

    try {
      if (p_Ele.HasAttribute(p_Name)) {
        i_Value             = Int32.Parse(p_Ele.GetAttribute(p_Name)); } }
    catch {
      i_Value               = p_Default; }
    return i_Value;
  }


  public bool AttrToBool(XmlElement p_Ele, string p_Name, bool p_Default)
  {
    bool            i_Value             = p_Default;
    string          i_Text;

    try {
      if (p_Ele.HasAttribute(p_Name)) {
        i_Text              = p_Ele.GetAttribute(p_Name).ToLower();
        if (i_Text == "true" || i_Text == "yes" || i_Text == "1") {
          i_Value           = true; }
        else {
          i_Value           = false; } } }
    catch {
      i_Value               = p_Default; }
    return i_Value;
  }


  public DateTime AttrToDate(XmlElement p_Ele, string p_Name, DateTime p_Default)
  {
    DateTime        i_Value             = p_Default;
    string          i_Text;

    try {
      if (p_Ele.HasAttribute(p_Name)) {
        i_Text              = p_Ele.GetAttribute(p_Name);
        i_Value             = DateTime.Parse(i_Text); } }
    catch {
      i_Value               = p_Default; }
    return i_Value;
  }

}