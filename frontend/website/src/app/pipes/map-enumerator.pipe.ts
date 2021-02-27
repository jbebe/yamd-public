import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'mapEnumerator'
})
export class MapEnumeratorPipe implements PipeTransform {

  // foo.bar.baz --> root['foo']['bar']['baz']
  static getPropertyRecursively(root: any, path: string): any {
    const pathParts = path.split('.');
    let currentRoot = root;
    for (const prop of pathParts){
      currentRoot = currentRoot[prop];
    }

    return currentRoot;
  }

  transform<T>(value: Map<string, T>, orderByProperty: string, reverse: boolean = false): T[] {
    const values = Array.from(value.values());
    const reverseValue = reverse ? -1 : 1;
    const getValue = orderByProperty.includes('.')
      ? (root: any, prop: string) => MapEnumeratorPipe.getPropertyRecursively(root, prop)
      : (root: any, prop: string) => root[prop];

    values.sort((a, b) => {
      const aVal = getValue(a, orderByProperty);
      const bVal = getValue(b, orderByProperty);
      return reverseValue * aVal.localeCompare(bVal);
    });

    return values;
  }

}
