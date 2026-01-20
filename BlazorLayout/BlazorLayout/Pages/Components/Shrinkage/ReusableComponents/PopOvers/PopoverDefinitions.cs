namespace BlazorLayout.Pages.Components.Shrinkage.ReusableComponents.PopOvers;


public static class PopoverDefinitions
{
    public static PopoverCardModel PaidTime => new()
    {
        Id = "popover-paid-time",
        Title = "Paid Time (Bezahlte Stunden)",
        Description = """

                      Unter bezahlte Stunden werden die mit dem Arbeitgeber vereinbarten vertraglichen Arbeitsstunden verstanden.
                      Im Folgenden werden verschiedene Situationen aufgeführt.
                      Dabei gilt: Die bezahlten Stunden orientieren sich an der vertraglich festgelegten Arbeitszeit,
                      z. B. Vollzeitkraft = 40 Std./Woche, Teilzeit = 20, 30 oder individuell vereinbart.
                      """,

        Items =
        [
            "Wenn der User von 8:00 Uhr morgens bis 16:30 Uhr arbeitet, hat er 8 bezahlte Stunden gearbeitet und 30 Minuten pausiert." +
            "Daher sollten hier 8:00 Stunden eingetragen worden sein.",

            "Wenn der User von 8:00 bis 14:00 Uhr arbeitet und 2 Stunden Freizeitausgleich nimmt, hat er trotzdem 8 bezahlte Arbeitsstunden." +
            "Daher sollten hier 8:00 Stunden eingetragen worden sein.",

            "Wenn der User von 8:00 bis 12:00 Uhr arbeitet, anschließend 3:00 Stunden private Angelegenheiten erledigt" +
            "und danach noch einmal 4:00 Stunden arbeitet, sollten hier insgesamt 8:00 Stunden eingetragen worden sein.",

            "Wenn der User von 8:00 bis 17:30 Uhr arbeitet, sollte hier 8:00 Stunden eingetragen worden sein.",

            "Wenn der User eine Teilzeitkraft ist (20 Stunden/Woche → 4 Stunden pro Tag) und von 8:00 bis 12:00 Uhr gearbeitet hat," +
            "sollten hier 4:00 Stunden eingetragen worden sein.",

            "Wenn der User eine Vollzeit- oder Teilzeitkraft ist und sich beim Teamleiter krankmeldet," +
            "trägt der Teamleiter die entsprechende bezahlte Zeit ein.",
        ],

        Warning = "WICHTIG! Mitarbeitende außerhalb der Entgeltfortzahlung sowie im unbezahlten Urlaub werden nicht erfasst.",
    };

    public static PopoverCardModel PaidTimeOff => new()
    {
        Id = "popover-paid-time-off",
        Title = "Paid Time Off (Freizeitausgleich)",
        Description = "Die Stunden welche der User aufgrund Ausgleichs des Arbeit-Zeitkontos nimmt werden als Freizeitausgleich bezeichnet. Beispiel:",
        Items =
        [
            "Wenn der User 2 Stunden von seinem Zeitkonto nutzen möchte",
            "Wenn der User Minusstunden aufbaut",
        ],
    };

    public static PopoverCardModel PaidOvertime => new()
    {
        Id = "popover-overtime",
        Title = "Over Time (Überstunden)",
        Description = "Die Stunden welche über die Arbeitszeit hinaus gehen",
    };

    public static PopoverCardModel VacationTime => new()
    {
        Id = "popover-vacation_time",
        Title = "Vacation Time (Urlaubsstunden)",
        Description = "Die Stunden welche der User bezahlte Urlaubsstunden nimmt, Beispiel:",
        Items = new List<string>
        {
            "Jahresurlaub",
            "Bildungsurlaub",
            "Sonderurlaub",
            "sonstige bezahlte Freistellung",
        },
        Warning = "WICHTIG!!!\r\nMitarbeitende in unbezahltem Urlaub werden nicht erfasst",
    };

    public static PopoverCardModel Others => new()
    {
        Id = "popover-others",
        Title = "Others (Sonstige nicht produktive Tätigkeiten)",
        Description = "Stunden die der User mit sonstigen, nicht produktiven Tätigkeiten verbringt Beispiel:",
        Items =
        [
            "ASA-Sitzungen",
            "Erste-Hilfe-Einsatz",
            "BR-Tätigkeit",
            "MA-Gespräch mit BR",
            "Dienstfahrten",
            "Erledigungen im Haus",
            "Themen die keiner anderen Kategorie zuzuordnen sind. Im Zweifel Rücksprache bei TLO / WFM",
        ],
    };

    public static PopoverCardModel Meeting => new()
    {
        Id = "popover-meeting",
        Title = "Meeting",
        Description = "Stunden die der User in Meetings verbringt",
        Items = new List<string>
        {
            "Jour Fixe",
            "Let's Talk-Gespräche",
            "One to One",
            "Teammeeting",
            "Abteilungsmeeting",
            "KV-Meeting",
            "Allhands Calls",
        },
    };

    public static PopoverCardModel TrainingOrCoaching => new()
    {
        Id = "popover-training-or-coaching",
        Title = "Training / Coaching",
        Description = "Stunden die der User für Training / Coaching benötigt , Beispiel :",
        Items =
        [
            "Einarbeitung (Einarbeitende und einzuarbeitende Mitarbeitende)",
            "Einarbeitung in neue Themen",
            "E-Learning, Schulungen",
            "Side by Side",
            "Abteilungsmeeting",
            "Coaching durch Trainer/TL",
            "Berufsschule (bei ganztägiger Abwesenheit, Pflege duch TLO)",
        ],
    };

    public static PopoverCardModel BusinessInterruption => new()
    {
        Id = "popover-business-interruption",
        Title = "Business interruption",
        Description = "Stunden in dem der User aufgrund von Betriebsunterbrechungen verhindert ist , Beispiel :",
        Items =
        [
            "Probleme mit RM-Software",
            "Probleme mit sonstiger IT-Soft- oder Hardware",
            "Probleme mit DMS",
            "Probleme mit dem Drucker",
            "Probleme mit dem Rechner",
            "Probleme mit den Anwendungsprogrammen",
            "Probleme mit der Telefonanlage",
            "Wartungsarbeiten jeglicher Art",
            "sonstige Unterbrechungen wie Austausch von IT-Equipment, Brandschutzübung, Evakuierung, Umzug\r\n",
        ],
    };

    public static PopoverCardModel Projects => new()
    {
        Id = "popover-projects",
        Title = "Projects (Projektarbeit)",
        Description = "Stunden in dem der User in einem Projekt unterstützt hat."
    };

    public static PopoverCardModel Loans => new()
    {
        Id = "popover-loan",
        Title = "Loan (Unterstützung anderer Teams)",
        Description = "Stunden die der User andere Teams/Einheiten unterstützt"
    };

    public static PopoverCardModel ProductiveNotMeasurable => new()
    {
        Id = "popover-productive-not-measurable",
        Title = "Produktiv nicht messbar (sonstige, produktive Tätigkeiten)",
        Description = "Stunden die der User mit sonstigen, produktiven Tätigkeiten außerhalb LUI verbringt , Beispiel: ",
        Items =
        [
            "Druck- und Fax-Tätigkeiten",
            "Listenbearbeitung",
            "Materialverwaltung",
            "extra Wrap-up time (ACW)",
            "E-Mail-Bearbeitung",
            "manuelle Post- und Wiedervorlagenbearbeitung, wenn nicht in LUI bearbeitet",
            "Service-Center Tätigkeiten",
            "Post- und Wiedervorlagebearbeitung bei LUI-Ausfal",
            "produktive Themen, die keiner anderen Kategorie zuzuordnen sind. Im Zweifel Rücksprache bei TLO / WFM",
        ],
    };
}


