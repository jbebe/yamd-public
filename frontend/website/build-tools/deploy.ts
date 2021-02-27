const { resolve, basename } = require('path');
const { readdir } = require('fs').promises;
const { BlobServiceClient } = require('@azure/storage-blob');
const { lookup } = require('mime-types');
const { SingleBar, Presets } = require('cli-progress');

async function getFilesAsync(dir: string): Promise<string[]> {
  const dirHandles = await readdir(dir, { withFileTypes: true });
  const files = await Promise.all(dirHandles.map((dirHandle) => {
    const res = resolve(dir, dirHandle.name);
    return dirHandle.isDirectory() ? getFilesAsync(res) : res;
  }));

  return Array.prototype.concat(...files);
}

(async () => {
  const progressBar = new SingleBar({}, Presets.shades_classic);
  const sasUrl = process.env.YAMD_WEBSITE_DEPLOY_TOKEN;
  const blobServiceClient = new BlobServiceClient(sasUrl);
  const containerClient = blobServiceClient.getContainerClient('');

  const files = await getFilesAsync('dist');
  progressBar.start(files.length, 0);
  for (let i = 0; i < files.length; ++i) {
    const filePath = files[i];
    const fileName = basename(filePath);
    const blobClient = containerClient.getBlockBlobClient(fileName);
    const response = await blobClient.uploadFile(filePath, {
      blobHTTPHeaders: {
        blobContentType: lookup(filePath)
      }
    });
    if (response.errorCode) throw new Error(`Azure error code: ${response.errorCode}`);
    progressBar.update(i + 1);
  }
  progressBar.stop();
})().then(() => {
  console.log('Deployment succeeded.');
}).catch((reason => {
  console.error('Exception: ', reason);
}));
