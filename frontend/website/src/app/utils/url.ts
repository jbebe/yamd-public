import {MediaType} from '../types/MediaType';
import {YamdException} from '../types/YamdException';
import {NormalizedUrl} from '../types/NormalizedUrl';

export function getUrlParts(url: string): URL | null {
  let urlObj: URL;

  try {
    urlObj = new URL(url);
  } catch (_) {
    return null;
  }

  return urlObj;
}

export function getMediaType(url: URL): MediaType {
  const hostname = url.hostname
    .replace(/^www\./, '')
    .toLowerCase();

  throw new YamdException('Unsupported media');
}

export function normalizeUrl(url: URL): NormalizedUrl {
  const withoutProtocolAndWww = url.href.replace(/^https?:\/\/(?:www\.)?/, '');
  return new NormalizedUrl(withoutProtocolAndWww);
}
