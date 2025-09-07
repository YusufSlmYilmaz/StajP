import React, { useEffect, useRef, useState } from "react";
import Map from "ol/Map";
import View from "ol/View";
import TileLayer from "ol/layer/Tile";
import OSM from "ol/source/OSM";
import { Vector as VectorLayer } from "ol/layer";
import { Vector as VectorSource } from "ol/source";
import WKT from "ol/format/WKT";
import { Fill, Stroke, Style, Circle as CircleStyle } from "ol/style";
import Draw from "ol/interaction/Draw";
import NavBar from "./NavBar";
import type { ObjectDto } from "../types";
import { Modify } from "ol/interaction";
import { Collection } from "ol";

const MapView: React.FC = () => {
  const mapRef = useRef<Map | null>(null);
  const [vectorSource] = useState(new VectorSource());
  const [drawType, setDrawType] = useState<"Point" | "LineString" | "Polygon">("Polygon");

  //  Liste için state'ler
  const [featuresList, setFeaturesList] = useState<any[]>([]);
  const [showList, setShowList] = useState(false);
  const [drawInteraction, setDrawInteraction] = useState<Draw | null>(null);
  const [selectedFeature, setSelectedFeature] = useState<any>(null);
  const [popupPosition, setPopupPosition] = useState< number[] | null>(null);
  const [modifyInteraction, setModifyInteraction] = useState<Modify | null>(null);
const [message, setMessage] = useState<string | null>(null);

  // Gezme modu için fonksiyon
const enableNavigation = () => {
  if (!mapRef.current) return;
  
  if (drawInteraction) {
    mapRef.current.removeInteraction(drawInteraction);
    setDrawInteraction(null);
  }
  
  setDrawType("Polygon"); 
};
  const updateFeaturesList = () => {
    const allFeatures = vectorSource.getFeatures();
    setFeaturesList([...allFeatures]);
  };
  

  const loadObjects = async () => {
    try {
      const res = await fetch("https://localhost:7073/api/object");
      if (!res.ok) throw new Error(res.statusText);
      const result = await res.json();

      const items: ObjectDto[] = (result.data ?? result).map((item: any) => ({
        id: item.id,
        name: item.name,
        wkt: item.geometry,
      }));

      const wkt = new WKT();
      vectorSource.clear();

      items.forEach((item) => {
        if (!item.wkt) return;
        const feature = wkt.readFeature(item.wkt, {
          dataProjection: "EPSG:4326",
          featureProjection: "EPSG:3857",
        });
        feature.setProperties({ name: item.name });
        if (item.id) feature.setId(item.id);
        vectorSource.addFeature(feature);
      });

      updateFeaturesList();
    } catch (err) {
      console.error("Veri çekme hatası:", err);
    }
  };

  useEffect(() => {
const rasterLayer = new TileLayer({
  source: new OSM({
    attributions: "", // boş string vererek yazıyı kaldırıyoruz
  }),
});
    const vectorLayer = new VectorLayer({
      source: vectorSource,
      style: (feature) => {
        const geomType = feature.getGeometry()?.getType();
        switch (geomType) {
          case "Point":
            return new Style({
              image: new CircleStyle({
                radius: 6,
                fill: new Fill({ color: "#ff0000b6" }),
                stroke: new Stroke({ color: "#8b0707ff", width: 1 }),
              }),
            });
          case "LineString":
            return new Style({ stroke: new Stroke({ color: "#28efe5fb", width: 3 }) });
          case "Polygon":
            return new Style({
              fill: new Fill({ color: "rgba(0, 255, 0, 0.3)" }),
              stroke: new Stroke({ color: "#078a0774", width: 2 }),
            });
          default:
            return new Style();
        }
      },
    });

    const map = new Map({
      target: "map",
      layers: [rasterLayer, vectorLayer],
      view: new View({
        center: [3630000, 4850000],
        zoom: 6,
      }),
    });

    mapRef.current = map;
    loadObjects();

    const draw = new Draw({
      source: vectorSource,
      type: drawType,
    });
    map.addInteraction(draw);

setDrawInteraction(draw);
map.on("singleclick", (evt) => {
  if (drawInteraction) return;
  const feature = map.forEachFeatureAtPixel(evt.pixel, (f) => f);
  if (feature) {
    setSelectedFeature(feature);
 setPopupPosition(map.getPixelFromCoordinate(evt.coordinate));  } else {
    setSelectedFeature(null);
    setPopupPosition(null);
  }
});



    draw.on("drawend", async (event) => {
      const feature = event.feature;
      const wkt = new WKT();

      const name = prompt("Bu nesne için isim girin:", "Yeni Nesne"); 
      if (!name) {
        vectorSource.removeFeature(feature);
        return;
      }
      feature.setProperties({ name });

      const geometryWKT = wkt.writeFeature(feature, {
        dataProjection: "EPSG:4326",
        featureProjection: "EPSG:3857",
      });

      try {
        const dto: ObjectDto = { name, wkt: geometryWKT };
        const res = await fetch("https://localhost:7073/api/object", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(dto),
        });
        const saved = await res.json();
        if (saved.isSuccess && saved.data?.id) {
          feature.setId(saved.data.id);
        } else {
          console.error("Kaydetme hatası:", saved.message);
        }
        updateFeaturesList();
      } catch (err) {
        console.error("Backend kaydetme hatası:", err);
      }
    });

    map.addInteraction(draw);

    return () => {
      map.setTarget(undefined);
      map.removeInteraction(draw);
    };
  }, [vectorSource, drawType]);

  const handleSave = async () => {
    const wkt = new WKT();
    const features = vectorSource.getFeatures();
    for (const feature of features) {
      if (!feature.getId()) {
        const geometryWKT = wkt.writeFeature(feature, {
          dataProjection: "EPSG:4326",
          featureProjection: "EPSG:3857",
        });
        try {
          const dto: ObjectDto = { name: feature.get("name") || "isimsiz", wkt: geometryWKT };
          const res = await fetch("https://localhost:7073/api/object", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dto),
          });
          const saved = await res.json();
          if (saved.isSuccess && saved.data?.id) feature.setId(saved.data.id);
        } catch (err) {
          console.error("Kaydetme hatası:", err);
        }
      }
    }
    updateFeaturesList();
    alert("Tüm yeni veriler kaydedildi.");
  };
const handleDeleteFeature = async (feature: any) => {
  if (!feature.getId()) {
    // Henüz kaydedilmemiş feature ise sadece kaldır
    vectorSource.removeFeature(feature);
    setSelectedFeature(null);
    setPopupPosition(null);
    updateFeaturesList();
    return;
  }

  try {
    const res = await fetch(`https://localhost:7073/api/object/${feature.getId()}`, {
      method: "DELETE",
    });
    const result = await res.json();
    if (result.isSuccess) {
      vectorSource.removeFeature(feature);
      setSelectedFeature(null);
      setPopupPosition(null);
      updateFeaturesList();
      alert("Silme işlemi başarılı.");
    } else {
      console.error("Backend silme hatası:", result.message);
    }
  } catch (err) {
    console.error("Silme hatası:", err);
  }
};

  // ✅ Zoom fonksiyonu
  const zoomToFeature = (feature: any) => {
    if (!mapRef.current) return;
    const view = mapRef.current.getView();
    const extent = feature.getGeometry().getExtent();
    view.fit(extent, { duration: 1000, padding: [50, 50, 50, 50],maxZoom: 9, });
  };

  return (
    <div>
      <NavBar
  onSave={handleSave}
  onReload={loadObjects}
  setDrawType={setDrawType}
  enableNavigation={enableNavigation}
  toggleList={() => setShowList(!showList)}
/>

{/* Harita alanı navbar altından başlasın */}
<div
  style={{ height: "calc(100vh - 50px)", width: "100%", marginTop: "50px" }}
  id="map"
></div>

      

      

      {/* Liste paneli */}
      {showList && (
  <div
    style={{
      position: "absolute",
      top: 60 + 50, // navbar altından başlasın
      right: 10,
      zIndex: 200,
      background: "white",
      padding: "10px",
      border: "1px solid #ccc",
      maxHeight: "300px",
      overflowY: "auto",
      width: "220px",
      borderRadius: "8px",
      boxShadow: "0 2px 6px rgba(0,0,0,0.2)",
    }}
  >
    <h4 style={{ margin: "0 0 10px 0", textAlign: "center" }}>Çizimler</h4>
    <ul style={{ listStyle: "none", padding: 0, margin: 0 }}>
      {featuresList.map((f, i) => (
        <li
          key={i}
          style={{
            display: "flex",
            flexDirection: "row",
            alignItems: "center",
            justifyContent: "space-between",
            marginBottom: "8px",
            wordBreak: "break-word", // uzun yazılar alt satıra geçer
            gap: "5px",
          }}
        >
          <span style={{ flex: 1 }}>{f.get("name") || "İsimsiz"}</span>
          <div style={{ display: "flex", gap: "5px" }}>
            <button
              onClick={() => zoomToFeature(f)}
              style={{
                padding: "4px 6px",
                border: "none",
                borderRadius: "4px",
                background: "#eee",
                cursor: "pointer",
              }}
            >
              👁️
            </button>
            <button
              onClick={() => handleDeleteFeature(f)}
              style={{
                padding: "4px 6px",
                border: "none",
                borderRadius: "4px",
                background: "#f88",
                cursor: "pointer",
              }}
            >
              🗑️
            </button>
          </div>
        </li>
      ))}
    </ul>
  </div>
)}


   {selectedFeature && popupPosition && (
  <div
    style={{
      position: "absolute",
      left: popupPosition[0],
      top: popupPosition[1],
      background: "white",
      border: "1px solid black",
      padding: "8px",
      borderRadius: "4px",
      zIndex: 1000,
    }}
  >
    <p><strong>İsim:</strong> {selectedFeature.get("name") || "İsimsiz"}</p>
    <p><strong>Tür:</strong> {selectedFeature.getGeometry().getType()}</p>
    <p>
      <strong>Koordinat:</strong>{" "}
      {selectedFeature.getGeometry().getCoordinates().join(", ")}
    </p>

    <button onClick={async () => {
  const newName = prompt("Yeni isim girin:", selectedFeature.get("name"));
  if (!newName) return;
  
  selectedFeature.set("name", newName);

  // Backend güncelle
  const wkt = new WKT();
  const geometryWKT = wkt.writeFeature(selectedFeature, {
    dataProjection: "EPSG:4326",
    featureProjection: "EPSG:3857",
  });

  try {
    await fetch(`https://localhost:7073/api/object/${selectedFeature.getId()}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name: newName, wkt: geometryWKT }),
    });
    updateFeaturesList();
    alert("İsim başarıyla güncellendi!");
  } catch (err) {
    console.error("Güncelleme hatası:", err);
  }
}}>
  İsmi Güncelle
</button>
 
    <button onClick={() => {
  if (!mapRef.current || !selectedFeature) return;

  // Eğer önceki modify varsa kaldır
  if (modifyInteraction) {
    mapRef.current.removeInteraction(modifyInteraction);
  }

  // Taşımak için Modify interaction oluştur
  const modify = new Modify({ features: new Collection([selectedFeature]) });
  mapRef.current.addInteraction(modify);
  setModifyInteraction(modify);

  // Taşıma sonrası backend kaydı
  modify.on("modifyend", async (evt) => {
    const modifiedFeature = evt.features.item(0);
    if (!modifiedFeature) return;

    const wkt = new WKT();
    const geometryWKT = wkt.writeFeature(modifiedFeature, {
      dataProjection: "EPSG:4326",
      featureProjection: "EPSG:3857",
    });

    try {
      await fetch(`https://localhost:7073/api/object/${modifiedFeature.getId()}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name: modifiedFeature.get("name"),
          wkt: geometryWKT
        }),
      });
      updateFeaturesList();
      setMessage("Taşıma işlemi backend'e kaydedildi!");
      setTimeout(() => setMessage(null), 1000); // 1 saniye sonra kaybolur

    } catch (err) {
      console.error("Taşıma güncelleme hatası:", err);
    }
  });

  alert("Taşıma modu aktif! Taşımayı tamamladıktan sonra haritaya tıklayın veya ESC ile iptal edin.");
}}>
  Taşı
</button>

    <button onClick={() => selectedFeature && handleDeleteFeature(selectedFeature)}>
  Sil
</button>

  </div>
)}


    </div>
  );
};

export default MapView;
