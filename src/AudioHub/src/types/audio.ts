export interface MediaUri {
  source: string;
  type: string;
  id: string;
  secondaryId?: string | null;
}

export interface AudioItem {
  uri?: MediaUri | null;
  album: string;
  title: string;
  artist: string;
  url: string;
  imageUrl: string;
}
