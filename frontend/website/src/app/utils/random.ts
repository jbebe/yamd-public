import {Md5} from 'ts-md5';
import {NormalizedUrl} from '../types/NormalizedUrl';

export function generateMediaId(normalizedUrl: NormalizedUrl): string {
  return Md5.hashAsciiStr(normalizedUrl.value) as string;
}
