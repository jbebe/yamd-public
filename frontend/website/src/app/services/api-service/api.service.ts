import { Injectable } from '@angular/core';
import {MediaEntity} from 'app/types/MediaEntity';
import {InvalidUrlException, ServerErrorException} from 'app/types/YamdException';
import {environment} from 'environments/environment';
import {SubmitResult} from 'app/types/SubmitResult';
import {getHttpStatus, isOfType, NativeType} from 'app/utils/type';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  constructor() { }

  private static async invokeApiAsync<T>(url: string, config?: RequestInit, processResponse?: (_: Response) => Promise<T>): Promise<T> {
    let response: Response;

    try {
      response = await fetch(url, config);
    }
    catch (exception){
      // If coming from a malformed URL, that's a TypeError
      if (isOfType(exception, NativeType.TypeError))
        exception = new InvalidUrlException('Supplied URL is invalid');

      throw exception;
    }

    if (!response.ok){
      // If response status has no 2xx format, still an error
      throw new ServerErrorException(
        `Service is down.\n` +
                `Error code: ${response.status}\n` +
                `Status: ${getHttpStatus(response.status)}`,
        await response.text());
    }

    try {
      return processResponse(response);
    }
    catch (exception){
      // If response body is not json for example, syntax error is thrown
      if (isOfType(exception, NativeType.SyntaxError))
        exception = new ServerErrorException('Unexpected response format');

      throw exception;
    }
  }

  public async submitMediaAsync(entity: MediaEntity): Promise<SubmitResult> {
    const result = await ApiService.invokeApiAsync(
      `${environment.apiUrl}/submit`,
      {
        body: JSON.stringify(entity),
        method: 'POST',
        headers: {
          'content-type': 'application/json',
          'accept': 'application/json',
        },
      },
      async r => await r.json()
    );

    return result as SubmitResult;
  }

  public async getTokenResponseAsync(token: string): Promise<string> {
    if (!environment.production) {
      const fragments = new URL(token);
      token = `${fragments.pathname}${fragments.search}`;
    }
    const result = await ApiService.invokeApiAsync(
      token,
      undefined,
      async r => await r.text());

    return result;
  }
}
