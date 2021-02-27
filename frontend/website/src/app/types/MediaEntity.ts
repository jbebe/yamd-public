import {MediaType} from './MediaType';
import {MediaState} from './MediaState';
import {parse} from 'fast-xml-parser';

type MediaXmlResponse = {
  feed: {
    entry: {
      content: {
        'm:properties': {
          'd:PartitionKey',
          'd:RowKey',
          'd:Timestamp': {
            '#text',
          },
          'd:CreatedDate',
          'd:State',
          'd:MediaUrl',
          'd:Type',
          'd:DownloadFormats',
        }
      }
    }
  }
};

type DownloadFormat = {
  resolution: string;
  downloadUrl: string;
};

export class MediaEntity {

  // metadata
  createdDate: string;
  state: MediaState;

  // generated md5 hash of normalized url
  id: string;

  // given
  mediaUrl: string;

  // calculated on frontend
  type: MediaType;

  // coming from api response
  title?: string;
  imageB64?: string;

  // coming from websocket event
  downloadFormats?: Array<DownloadFormat>;

  public static fromStorageXml(xml: string): MediaEntity {
    const responseObj: MediaXmlResponse = parse(xml);
    const properties = responseObj.feed.entry.content['m:properties'];

    return {
      createdDate: properties['d:CreatedDate'],
      downloadFormats: JSON.parse(properties['d:DownloadFormats']),
      id: properties['d:PartitionKey'],
      imageB64: properties['d:ImageB64'],
      mediaUrl: properties['d:MediaUrl'],
      state: properties['d:State'],
      title: properties['d:Title'],
      type: properties['d:Type'],
    };
  }
}
