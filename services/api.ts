import type { ObjectDto } from "../types";

// Backend'den tüm objeleri çek
export async function fetchObjects(): Promise<ObjectDto[]> {
  const res = await fetch("https://localhost:7073/api/object");

  if (!res.ok) {
    throw new Error("Veri alınamadı: " + res.statusText);
  }

  // Backend ObjectController içinden "data" döndürüyorsa
  const result = await res.json();
  return result.data ?? result; 
}
