# PcInfo - Informacje o komputerze
## Opis
PcInfo to aplikacja konsolowa napisana w języku C#, która automatycznie zbiera szczegółowe informacje na temat komputera, takie jak:

* Numer seryjny BIOS oraz numer produktu.
* Wersja systemu operacyjnego.
* Szczegóły dotyczące sprzętu.
* Lista drukarek podłączonych do komputera.
* Dyski sieciowe.
* Adresy MAC kart sieciowych.
* Daty ostatnich logowań użytkowników.
* Wyniki są zapisywane w pliku CSV, który jest nazwany na podstawie nazwy komputera.

## Funkcje
* PobierzNumerSeryjnyBIOSTaNumeryProduktu: Pobiera numer seryjny BIOS i numer produktu.
* PobierzWersjeSystemu: Zbiera informacje o wersji systemu operacyjnego.
* PobierzInformacjeSprzetowe: Gromadzi szczegóły na temat sprzętu, takie jak procesor i pamięć RAM.
* PobierzListeDrukarek: Pobiera listę zainstalowanych drukarek.
* PobierzDyskiSieciowe: Wyświetla zamontowane dyski sieciowe.
* PobierzAdresyMAC: Pobiera adresy MAC zainstalowanych kart sieciowych.
* PobierzDatyOstatnichLogowan: Wyświetla daty ostatnich logowań użytkowników.
* Wyniki zostaną zapisane do pliku CSV w lokalnym katalogu, w formacie NazwaKomputera_wyniki.csv.

## Wymagania
.NET Framework 4.7.2 lub nowszy
Uprawnienia administratora do uzyskania pełnych informacji o systemie

## Licencja
Projekt dostępny na licencji MIT.
