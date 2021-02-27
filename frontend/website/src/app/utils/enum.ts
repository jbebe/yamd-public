export function getEnumMap(typeObj: any): Map<string, string> {
  const props = Object.keys(typeObj);
  const map = new Map<string, string>();
  const half = Math.floor(props.length / 2);

  for (let i = 0; i < half; ++i){
    const valueIndex = i;
    const nameIndex = i + half;
    map.set(props[nameIndex], props[valueIndex]);
  }

  return map;
}
