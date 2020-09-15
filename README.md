# CefBot
- Program (bot) pobiera dane ze stron suplementy.pl / mpcforum.pl. Dane to suplementy z aktualnej wyprzedaży w przypadku pierwszego serwisu oraz najnowsze posty wraz z linkami do odpowiedzi na nie w przypadku drugiego serwisu.
- Każde uruchomienie bota nadpisuje bieżący plik jeśli istnieje, jeśli nie to tworzy nowy o takiej samej nazwie. Podanie nieprawidłowych danych logowania powoduje wyrzucenie wyjątku oraz nie tworzy/nadpisuje istniejącego pliku.
- Aby uruchomić program wchodzimy na stronę suplementy.pl/mpcforum i zakładamy tam konto. W pliku app.config rozwiązania CefSharp.MinimalExample.WinForms podmieniamy umieszczony tam email oraz hasło na takie jakie podaliśmy przy tworzeniu konta.
- Kompilujemy rozwiązanie, debugujemy aby doprowadzić do powstania aplikacji w podfolderze debug, w przeciwnym razie timer nie odnajdzie aplikacji bota. Następnie wchodzimy w rozwiązanie CefSharpBotWithTimer.sln i również je kompilujemy.
- Ustawiamy w nim dwie zmienne w funkcji main, które określą o której godzinie bot ma się uruchomić po raz pierwszy. Domyślnie ustawiona została tam godzina.
- W pliku app.config należy umieścić jako wartość przy kluczu "url" jeden z dwóch linków do obsługiwanych serwisów. Program na podstawie danego linku rozpozna z jakiego serwisu ma skorzystać.
- Po wykonaniu wszystkich czynności następuje wylogowanie z konta i zamknięcie aplikacji.
- Program posiada oddzielny parser danych dla każdego z serwisów.
