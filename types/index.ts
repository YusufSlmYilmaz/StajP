export interface ObjectDto {
  id?: number;   // DB otomatik arttırıyor ama GET’lerde geliyor
  name: string;
  wkt: string;   // ✅ backend ile aynı isim olmalı
}
