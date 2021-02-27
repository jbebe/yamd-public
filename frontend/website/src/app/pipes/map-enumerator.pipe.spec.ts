import { MapEnumeratorPipe } from './map-enumerator.pipe';

describe('MapEnumeratorPipe', () => {
  it('create an instance', () => {
    const pipe = new MapEnumeratorPipe();

    const map = new Map<string, any>([['a', { x: '1' }], ['b', { x: '2'}]]);
    const result = pipe.transform(map, 'x', true);

    expect(result[0].x).toBe('2');
    expect(result[1].x).toBe('1');
  });
});
