import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'fileName',
  standalone: true,
})
export class FileNamePipe implements PipeTransform {
  transform(value?: string | null): string | null {
    if (!value) {
      return null;
    }

    return value.replace(/^.*[\\/]/, '');
  }
}
