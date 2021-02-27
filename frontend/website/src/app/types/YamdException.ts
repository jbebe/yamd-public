export class YamdException {

  constructor(
    public message: string,
    public payload?: string,
  ) {
  }
}

export class InvalidUrlException extends YamdException {

  constructor(message: string) {
    super(message);
  }

}

export class ServerErrorException extends YamdException {

  constructor(message: string, payload?: string) {
    super(message, payload);
  }

}
